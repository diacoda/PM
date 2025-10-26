using System;
using System.Collections.Generic;
using System.Linq;
using model.Domain.Entities;
using model.Domain.Values;
using model.Repositories;

namespace model.Services
{
    /// <summary>
    /// Produces and stores valuation snapshots using the existing ValuationService.
    /// Adds per-snapshot component breakdown (SecuritiesValue, CashValue, IncomeForDay).
    /// Also persists Asset-Class slices with Percentage weights.
    /// </summary>
    public class ValuationManager
    {
        private readonly ValuationService _valuationService;
        private readonly ValuationRepository _repository;

        public ValuationManager(ValuationService valuationService, ValuationRepository repository)
        {
            _valuationService = valuationService;
            _repository = repository;
        }

        // ---------------------------------------------------------------------
        // TOTAL SNAPSHOTS (Portfolio / Account)
        // ---------------------------------------------------------------------

        public void GenerateAndStorePortfolioValuations(
            Portfolio portfolio,
            DateTime startDate,
            DateTime endDate,
            Currency reportingCurrency,
            ValuationPeriod period)
        {
            foreach (var date in GetDatesByPeriod(startDate, endDate, period))
            {
                // TOTAL portfolio value in reporting currency
                var total = _valuationService.CalculatePortfolioValue(portfolio, date, reportingCurrency);

                // CASH = sum of all accounts' cash holdings (CASH.*) as of date
                decimal cashAmt = 0m;
                foreach (var account in portfolio.Accounts)
                {
                    cashAmt += SumAccountCash(account, date, reportingCurrency);
                }

                var cash = new Money(cashAmt, reportingCurrency);
                var secs = new Money(total.Amount - cashAmt, reportingCurrency);

                // IncomeForDay = sum of same-day dividends recorded in reportingCurrency across accounts (net of withholding)
                decimal divNet = 0m;
                foreach (var account in portfolio.Accounts)
                {
                    divNet += SumAccountNetDividendsForDay(account, date, reportingCurrency);
                }

                var record = new ValuationRecord
                {
                    Date = date,
                    Period = period,
                    ReportingCurrency = reportingCurrency,
                    Value = total,
                    PortfolioId = portfolio.Id,
                    // components
                    SecuritiesValue = secs,
                    CashValue = cash,
                    IncomeForDay = divNet == 0m ? null : new Money(divNet, reportingCurrency)
                };

                _repository.Save(record);
            }
        }

        public void GenerateAndStoreAccountValuations(
            Account account,
            DateTime startDate,
            DateTime endDate,
            Currency reportingCurrency,
            ValuationPeriod period)
        {
            foreach (var date in GetDatesByPeriod(startDate, endDate, period))
            {
                // TOTAL account value in reporting currency
                var total = _valuationService.CalculateAccountValue(account, date, reportingCurrency);

                // CASH
                var cashAmt = SumAccountCash(account, date, reportingCurrency);
                var cash = new Money(cashAmt, reportingCurrency);
                var secs = new Money(total.Amount - cashAmt, reportingCurrency);

                // IncomeForDay (net dividends) in reporting currency
                var divNet = SumAccountNetDividendsForDay(account, date, reportingCurrency);

                var record = new ValuationRecord
                {
                    Date = date,
                    Period = period,
                    ReportingCurrency = reportingCurrency,
                    Value = total,
                    AccountId = account.Id,
                    // components
                    SecuritiesValue = secs,
                    CashValue = cash,
                    IncomeForDay = divNet == 0m ? null : new Money(divNet, reportingCurrency)
                };

                _repository.Save(record);
            }
        }

        // ---------------------------------------------------------------------
        // ASSET-CLASS SNAPSHOTS (Portfolio / Account) with Percentage
        // ---------------------------------------------------------------------

        public void GenerateAndStorePortfolioValuationsByAssetClass(
            Portfolio portfolio,
            DateTime startDate,
            DateTime endDate,
            Currency reportingCurrency,
            ValuationPeriod period)
        {
            foreach (var date in GetDatesByPeriod(startDate, endDate, period))
            {
                var total = _valuationService.CalculatePortfolioValue(portfolio, date, reportingCurrency).Amount;
                if (total <= 0m) continue;

                var byClass = AggregateByAssetClass(portfolio, date, reportingCurrency);
                foreach (var kvp in byClass)
                {
                    var value = kvp.Value;
                    var pct = value.Amount / total;

                    var record = new ValuationRecord
                    {
                        Date = date,
                        Period = period,
                        ReportingCurrency = reportingCurrency,
                        Value = value,
                        PortfolioId = portfolio.Id,
                        AssetClass = kvp.Key,
                        Percentage = pct
                    };
                    _repository.Save(record);
                }
            }
        }

        public void GenerateAndStoreAccountValuationsByAssetClass(
            Account account,
            DateTime startDate,
            DateTime endDate,
            Currency reportingCurrency,
            ValuationPeriod period)
        {
            foreach (var date in GetDatesByPeriod(startDate, endDate, period))
            {
                var total = _valuationService.CalculateAccountValue(account, date, reportingCurrency).Amount;
                if (total <= 0m) continue;

                var byClass = AggregateByAssetClass(account, date, reportingCurrency);
                foreach (var kvp in byClass)
                {
                    var value = kvp.Value;
                    var pct = value.Amount / total;

                    var record = new ValuationRecord
                    {
                        Date = date,
                        Period = period,
                        ReportingCurrency = reportingCurrency,
                        Value = value,
                        AccountId = account.Id,
                        AssetClass = kvp.Key,
                        Percentage = pct
                    };
                    _repository.Save(record);
                }
            }
        }

        // ---------------------------------------------------------------------
        // Helpers (internal)
        // ---------------------------------------------------------------------

        private IEnumerable<DateTime> GetDatesByPeriod(DateTime start, DateTime end, ValuationPeriod period)
        {
            var dates = new List<DateTime>();
            var current = start.Date;
            while (current <= end.Date)
            {
                dates.Add(current);
                current = period switch
                {
                    ValuationPeriod.Daily     => current.AddDays(1),
                    ValuationPeriod.Monthly   => current.AddMonths(1),
                    ValuationPeriod.Quarterly => current.AddMonths(3),
                    ValuationPeriod.Yearly    => current.AddYears(1),
                    _ => throw new ArgumentOutOfRangeException(nameof(period), "Unsupported valuation period")
                };
            }
            return dates;
        }

        private Dictionary<AssetClass, Money> AggregateByAssetClass(Account account, DateTime date, Currency reportingCurrency)
        {
            var result = new Dictionary<AssetClass, Money>();
            foreach (var holding in account.Holdings)
            {
                var value = _valuationService.CalculateHoldingValue(holding, date, reportingCurrency);
                var cls = holding.Instrument.AssetClass;
                if (result.TryGetValue(cls, out var existing))
                    result[cls] = new Money(existing.Amount + value.Amount, reportingCurrency);
                else
                    result[cls] = value;
            }
            return result;
        }

        private Dictionary<AssetClass, Money> AggregateByAssetClass(Portfolio portfolio, DateTime date, Currency reportingCurrency)
        {
            var result = new Dictionary<AssetClass, Money>();
            foreach (var account in portfolio.Accounts)
            {
                var accAgg = AggregateByAssetClass(account, date, reportingCurrency);
                foreach (var kvp in accAgg)
                {
                    if (result.TryGetValue(kvp.Key, out var existing))
                        result[kvp.Key] = new Money(existing.Amount + kvp.Value.Amount, reportingCurrency);
                    else
                        result[kvp.Key] = kvp.Value;
                }
            }
            return result;
        }

        private decimal SumAccountCash(Account account, DateTime date, Currency reportingCurrency)
        {
            decimal cash = 0m;
            foreach (var h in account.Holdings.Where(h => h.Instrument.Symbol.Code.StartsWith("CASH.", StringComparison.OrdinalIgnoreCase)))
            {
                var val = _valuationService.CalculateHoldingValue(h, date, reportingCurrency);
                cash += val.Amount;
            }
            return cash;
        }

        private decimal SumAccountNetDividendsForDay(Account account, DateTime date, Currency reportingCurrency)
        {
            // KISS: only include dividends already denominated in the reporting currency
            // (If you later want cross-ccy inclusion, we can convert via the valuation service's FX provider.)
            var net = account.Transactions
                .Where(t => t.Type == TransactionType.Dividend && t.Date.Date == date.Date)
                .Where(t => t.Amount.Currency == reportingCurrency)
                .Sum(t => (t.Amount.Amount - (t.Costs?.Amount ?? 0m)));
            return net;
        }
    }
}