using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Application.Interfaces;
using PM.Domain.Enums;
using PM.SharedKernel;
using PM.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PM.Application.Services
{
    /// <summary>
    /// Daily-focused Valuation Calculator.
    /// Produces and stores valuation snapshots for a single date.
    /// Adds per-snapshot component breakdown (SecuritiesValue, CashValue, IncomeForDay)
    /// and optionally persists Asset-Class slices with Percentage weights.
    /// </summary>
    public class DailyValuationCalculator : IValuationService
    {
        private readonly IPricingService _pricingService;
        private readonly IValuationRepository _repository;
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IAccountRepository _accountRepository;

        public DailyValuationCalculator(
            IPricingService pricingService,
            IValuationRepository repository,
            IPortfolioRepository portfolioRepository,
            IAccountRepository accountRepository)
        {
            _pricingService = pricingService;
            _repository = repository;
            _portfolioRepository = portfolioRepository;
            _accountRepository = accountRepository;
        }

        public async Task<IEnumerable<ValuationRecord>> GetByPortfolioAsync(
            int portfolioId,
            ValuationPeriod period,
            CancellationToken ct = default)
        {
            if (portfolioId <= 0)
                throw new ArgumentException("Invalid portfolio ID", nameof(portfolioId));

            return await _repository.GetByPortfolioAsync(portfolioId, period, ct);
        }

        public async Task GenerateAndStorePortfolioValuations(
            int portfolioId,
            DateOnly date,
            Currency reportingCurrency,
            ValuationPeriod period,
            CancellationToken ct = default)
        {
            var portfolio = await _portfolioRepository.GetByIdWithIncludesAsync(
                portfolioId,
                new[] { IncludeOption.Accounts, IncludeOption.Holdings },
                ct);

            if (portfolio is null) return;

            var total = await _pricingService.CalculatePortfolioValueAsync(portfolio, date, reportingCurrency, ct);

            decimal cashAmt = 0m;
            foreach (var account in portfolio.Accounts)
                cashAmt += await SumAccountCashAsync(account, date, reportingCurrency, ct);

            var cash = new Money(cashAmt, reportingCurrency);
            var secs = new Money(total.Amount - cashAmt, reportingCurrency);

            decimal divNet = 0m;
            foreach (var account in portfolio.Accounts)
                divNet += SumAccountNetDividendsForDay(account, date, reportingCurrency);

            var record = new ValuationRecord
            {
                Date = date,
                Period = period,
                ReportingCurrency = reportingCurrency,
                Value = total,
                PortfolioId = portfolio.Id,
                SecuritiesValue = secs,
                CashValue = cash,
                IncomeForDay = divNet == 0m ? null : new Money(divNet, reportingCurrency)
            };

            await _repository.SaveAsync(record, ct);
        }

        public async Task GenerateAndStoreAccountValuations(
            int portfolioId,
            int accountId,
            DateOnly date,
            Currency reportingCurrency,
            ValuationPeriod period,
            CancellationToken ct = default)
        {
            var account = await _accountRepository.GetByIdWithIncludesAsync(
                accountId,
                new[] { IncludeOption.Holdings },
                ct);

            if (account is null) return;

            var total = await _pricingService.CalculateAccountValueAsync(account, date, reportingCurrency, ct);

            var cashAmt = await SumAccountCashAsync(account, date, reportingCurrency, ct);
            var cash = new Money(cashAmt, reportingCurrency);
            var secs = new Money(total.Amount - cashAmt, reportingCurrency);

            var divNet = SumAccountNetDividendsForDay(account, date, reportingCurrency);

            var record = new ValuationRecord
            {
                Date = date,
                Period = period,
                ReportingCurrency = reportingCurrency,
                Value = total,
                AccountId = account.Id,
                SecuritiesValue = secs,
                CashValue = cash,
                IncomeForDay = divNet == 0m ? null : new Money(divNet, reportingCurrency)
            };

            await _repository.SaveAsync(record, ct);
        }

        public async Task GenerateAndStorePortfolioValuationsByAssetClass(
            int portfolioId,
            DateOnly date,
            Currency reportingCurrency,
            ValuationPeriod period,
            CancellationToken ct = default)
        {
            var portfolio = await _portfolioRepository.GetByIdWithIncludesAsync(
                portfolioId,
                new[] { IncludeOption.Accounts, IncludeOption.Holdings },
                ct);

            if (portfolio is null) return;

            var totalMoney = await _pricingService.CalculatePortfolioValueAsync(portfolio, date, reportingCurrency, ct);
            var total = totalMoney.Amount;
            if (total <= 0m) return;

            var byClass = await AggregateByAssetClassAsync(portfolio, date, reportingCurrency, ct);
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
                await _repository.SaveAsync(record, ct);
            }
        }

        public async Task GenerateAndStoreAccountValuationsByAssetClass(
            int portfolioId,
            int accountId,
            DateOnly date,
            Currency reportingCurrency,
            ValuationPeriod period,
            CancellationToken ct = default)
        {
            var account = await _accountRepository.GetByIdWithIncludesAsync(
                accountId,
                new[] { IncludeOption.Holdings },
                ct);

            if (account is null) return;

            var totalMoney = await _pricingService.CalculateAccountValueAsync(account, date, reportingCurrency, ct);
            var total = totalMoney.Amount;
            if (total <= 0m) return;

            var byClass = await AggregateByAssetClassAsync(account, date, reportingCurrency, ct);
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
                await _repository.SaveAsync(record, ct);
            }
        }

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------

        private async Task<Dictionary<AssetClass, Money>> AggregateByAssetClassAsync(Account account, DateOnly date, Currency reportingCurrency, CancellationToken ct = default)
        {
            var result = new Dictionary<AssetClass, Money>();
            foreach (var holding in account.Holdings)
            {
                var value = await _pricingService.CalculateHoldingValueAsync(holding, date, reportingCurrency, ct);
                var cls = holding.Asset.AssetClass;
                if (result.TryGetValue(cls, out var existing))
                    result[cls] = new Money(existing.Amount + value.Amount, reportingCurrency);
                else
                    result[cls] = value;
            }
            return result;
        }

        private async Task<Dictionary<AssetClass, Money>> AggregateByAssetClassAsync(Portfolio portfolio, DateOnly date, Currency reportingCurrency, CancellationToken ct = default)
        {
            var result = new Dictionary<AssetClass, Money>();
            foreach (var account in portfolio.Accounts)
            {
                var accAgg = await AggregateByAssetClassAsync(account, date, reportingCurrency, ct);
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

        private async Task<decimal> SumAccountCashAsync(Account account, DateOnly date, Currency reportingCurrency, CancellationToken ct = default)
        {
            decimal cash = 0m;
            foreach (var h in account.Holdings.Where(h => h.Asset.AssetClass == AssetClass.Cash))
            {
                var val = await _pricingService.CalculateHoldingValueAsync(h, date, reportingCurrency, ct);
                cash += val.Amount;
            }
            return cash;
        }

        private decimal SumAccountNetDividendsForDay(Account account, DateOnly date, Currency reportingCurrency)
        {
            var net = account.Transactions
                .Where(t => t.Type == TransactionType.Dividend && t.Date == date)
                .Where(t => t.Amount.Currency == reportingCurrency)
                .Sum(t => (t.Amount.Amount - (t.Costs?.Amount ?? 0m)));
            return net;
        }
    }
}
