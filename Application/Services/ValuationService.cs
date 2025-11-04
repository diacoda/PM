using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Application.Interfaces;
using PM.Domain.Enums;
using PM.SharedKernel;
using PM.Domain.Mappers;
using PM.DTO;

namespace PM.Application.Services;

/// <summary>
/// Produces and stores valuation snapshots using the existing ValuationService.
/// Adds per-snapshot component breakdown (SecuritiesValue, CashValue, IncomeForDay).
/// Also persists Asset-Class slices with Percentage weights.
/// </summary>
public class ValuationService : IValuationService
{
    private readonly IPricingService _pricingService;
    private readonly IValuationRepository _repository;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IAccountRepository _accountRepository;

    public ValuationService(IPricingService pricingService, IValuationRepository repository, IPortfolioRepository portfolioRepository, IAccountRepository accountRepository)
    {
        _pricingService = pricingService;
        _repository = repository;
        _portfolioRepository = portfolioRepository;
        _accountRepository = accountRepository;
    }

    public async Task<IEnumerable<ValuationRecord>> GetByPortfolioAsync(int portfolioId, ValuationPeriod period, CancellationToken ct = default)
    {
        // optional: business logic, validation, caching, transformations, etc.
        if (portfolioId <= 0)
            throw new ArgumentException("Invalid portfolio ID", nameof(portfolioId));

        return await _repository.GetByPortfolioAsync(portfolioId, period, ct);
    }
    // ---------------------------------------------------------------------
    // TOTAL SNAPSHOTS (Portfolio / Account)
    // ---------------------------------------------------------------------

    public async Task GenerateAndStorePortfolioValuations(
        int portfolioId,
        DateTime startDate,
        DateTime endDate,
        Currency reportingCurrency,
        ValuationPeriod period,
        CancellationToken ct = default)
    {
        IncludeOption[] includes = { IncludeOption.Accounts, IncludeOption.Holdings };

        var portfolio = await _portfolioRepository.GetByIdWithIncludesAsync(portfolioId, includes, ct);
        if (portfolio is null)
            return;

        foreach (var date in GetDatesByPeriod(startDate, endDate, period))
        {
            // TOTAL portfolio value in reporting currency
            var total = await _pricingService.CalculatePortfolioValueAsync(portfolio, date, reportingCurrency, ct);

            // CASH = sum of all accounts' cash holdings (CASH.*) as of date
            decimal cashAmt = 0m;
            foreach (var account in portfolio.Accounts)
            {
                cashAmt += await SumAccountCashAsync(account, date, reportingCurrency, ct);
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

            await _repository.SaveAsync(record, ct);
        }
    }

    public async Task GenerateAndStoreAccountValuations(
        int portfolioId,
        int accountId,
        DateTime startDate,
        DateTime endDate,
        Currency reportingCurrency,
        ValuationPeriod period,
        CancellationToken ct = default)
    {
        IncludeOption[] includes = { IncludeOption.Holdings };
        Account? account = await _accountRepository.GetByIdWithIncludesAsync(accountId, includes, ct);
        if (account is null) return;

        foreach (var date in GetDatesByPeriod(startDate, endDate, period))
        {
            // TOTAL account value in reporting currency
            var total = await _pricingService.CalculateAccountValueAsync(account, date, reportingCurrency, ct);

            // CASH
            var cashAmt = await SumAccountCashAsync(account, date, reportingCurrency, ct);
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

            await _repository.SaveAsync(record, ct);
        }
    }

    // ---------------------------------------------------------------------
    // ASSET-CLASS SNAPSHOTS (Portfolio / Account) with Percentage
    // ---------------------------------------------------------------------

    public async Task GenerateAndStorePortfolioValuationsByAssetClass(
        int portfolioId,
        DateTime startDate,
        DateTime endDate,
        Currency reportingCurrency,
        ValuationPeriod period,
        CancellationToken ct = default)
    {
        IncludeOption[] includes = { IncludeOption.Accounts, IncludeOption.Holdings };

        var portfolio = await _portfolioRepository.GetByIdWithIncludesAsync(portfolioId, includes, ct);
        if (portfolio is null)
            return;

        foreach (var date in GetDatesByPeriod(startDate, endDate, period))
        {
            var totalMoney = await _pricingService.CalculatePortfolioValueAsync(portfolio, date, reportingCurrency, ct);
            var total = totalMoney.Amount;
            if (total <= 0m) continue;

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
    }

    public async Task GenerateAndStoreAccountValuationsByAssetClass(
        int portfolioId,
        int accountId,
        DateTime startDate,
        DateTime endDate,
        Currency reportingCurrency,
        ValuationPeriod period,
        CancellationToken ct = default)
    {
        IncludeOption[] includes = { IncludeOption.Holdings };
        Account? account = await _accountRepository.GetByIdWithIncludesAsync(accountId, includes, ct);
        if (account is null) return;

        foreach (var date in GetDatesByPeriod(startDate, endDate, period))
        {
            var totalMoney = await _pricingService.CalculateAccountValueAsync(account, date, reportingCurrency, ct);
            var total = totalMoney.Amount;
            if (total <= 0m) continue;

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
                ValuationPeriod.Daily => current.AddDays(1),
                ValuationPeriod.Monthly => current.AddMonths(1),
                ValuationPeriod.Quarterly => current.AddMonths(3),
                ValuationPeriod.Yearly => current.AddYears(1),
                _ => throw new ArgumentOutOfRangeException(nameof(period), "Unsupported valuation period")
            };
        }
        return dates;
    }

    private async Task<Dictionary<AssetClass, Money>> AggregateByAssetClassAsync(Account account, DateTime date, Currency reportingCurrency, CancellationToken ct = default)
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

    private async Task<Dictionary<AssetClass, Money>> AggregateByAssetClassAsync(Portfolio portfolio, DateTime date, Currency reportingCurrency, CancellationToken ct = default)
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

    private async Task<decimal> SumAccountCashAsync(Account account, DateTime date, Currency reportingCurrency, CancellationToken ct = default)
    {
        decimal cash = 0m;
        foreach (var h in account.Holdings.Where(h => h.Asset.AssetClass == AssetClass.Cash))
        {
            var val = await _pricingService.CalculateHoldingValueAsync(h, date, reportingCurrency, ct);
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
