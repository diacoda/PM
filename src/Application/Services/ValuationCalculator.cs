using PM.Application.Interfaces;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.SharedKernel;

namespace PM.Application.Services;

public class ValuationCalculator : IValuationCalculator
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IValuationService _valuationService;

    public ValuationCalculator(IPortfolioRepository portfolioRepository, IValuationService valuationService)
    {
        _portfolioRepository = portfolioRepository;
        _valuationService = valuationService;
    }

    private Valuation GenerateAggregateValuation(
        decimal total,
        decimal cash,
        decimal income,
        Currency reportingCurrency)
    {
        Money totalValue = new Money(total, reportingCurrency);
        Money cashValue = new Money(cash, reportingCurrency);
        Money securitiesValue = new Money(total - cash, reportingCurrency);
        Money incomeForDay = income == 0m ? null : new Money(income, reportingCurrency);
        return new Valuation(totalValue, cashValue, securitiesValue, incomeForDay, reportingCurrency, AssetClass.None, 0m);
    }

    private ValuationSnapshot GenerateAggregateSnapshot(
        EntityKind kind,
        string? owner,
        DateOnly date,
        Currency reportingCurrency,
        decimal total,
        decimal cash,
        decimal income)
    {
        return new ValuationSnapshot
        {
            Kind = kind,
            Owner = owner,
            Date = date,
            ReportingCurrency = reportingCurrency,
            Value = new Money(total, reportingCurrency),
            CashValue = new Money(cash, reportingCurrency),
            SecuritiesValue = new Money(total - cash, reportingCurrency),
            IncomeForDay = income == 0m ? null : new Money(income, reportingCurrency)
        };
    }

    private List<Valuation> GenerateAggregateAssetClassValuations(
        Currency reportingCurrency,
        Dictionary<AssetClass, Money> classes,
        decimal total)
    {
        var denom = total <= 0 ? 1 : total;

        return classes.Select(kvp =>
            new Valuation(
                kvp.Value,
                new Money(0m, reportingCurrency),
                new Money(0m, reportingCurrency),
                new Money(0m, reportingCurrency),
                reportingCurrency,
                kvp.Key,
                kvp.Value.Amount / denom))
            .ToList();
    }

    public async Task CalculateAccountValuationAsync(DateOnly date, int portfolioId, int accountId, string reportingCurrency, IEnumerable<ValuationPeriod> periods, CancellationToken ct = default)
    {
        Currency reportCurrency = new Currency(reportingCurrency);
        var accountValuation = await _valuationService.GenerateAccountValuation(
            portfolioId, accountId, date, reportCurrency, ct);

        foreach (var period in periods)
        {
            await _valuationService.StoreAccountValuation(portfolioId, accountId, accountValuation, date, period, ct);
        }

        var accountAssetClassValuation = await _valuationService.GenerateAccountAssetClassValuation(
            portfolioId, accountId, date, reportCurrency, ct);

        foreach (var period in periods)
        {
            await _valuationService.StoreAccountAssetClassValuation(portfolioId, accountId, accountAssetClassValuation, date, period, ct);
        }
    }

    public async Task CalculateValuationsAsync(DateOnly date, IEnumerable<ValuationPeriod> periods, CancellationToken ct = default)
    {
        var portfolios = await _portfolioRepository.ListWithIncludesAsync(
            new[] { IncludeOption.Accounts, IncludeOption.Holdings }, ct);

        var reportingCurrency = Currency.CAD;

        //  ACCUMULATORS (Owner + Estate)
        var ownerTotals = new Dictionary<string, (decimal total, decimal cash, decimal income)>();
        var ownerByClass = new Dictionary<string, Dictionary<AssetClass, Money>>();

        decimal estateTotal = 0m;
        decimal estateCash = 0m;
        decimal estateIncome = 0m;
        var estateClassTotals = new Dictionary<AssetClass, Money>();

        // PHASE 1: Portfolio + Account Valuations
        foreach (var portfolio in portfolios)
        {
            // Compute portfolio snapshot once
            var portVal = await _valuationService.GeneratePortfolioValuation(
                portfolio.Id, date, reportingCurrency, ct);

            foreach (var period in periods)
            {
                await _valuationService.StorePortfolioValuation(portfolio.Id, portVal, date, period, ct);
            }

            // Accounts
            foreach (var account in portfolio.Accounts)
            {
                var accSnap = await _valuationService.GenerateAccountValuation(
                    portfolio.Id, account.Id, date, reportingCurrency, ct);

                foreach (var period in periods)
                {
                    await _valuationService.StoreAccountValuation(portfolio.Id, account.Id, accSnap, date, period, ct);
                }
                // account by class
                var accByClass = await _valuationService.GenerateAccountAssetClassValuation(
                    portfolio.Id, account.Id, date, reportingCurrency, ct);

                foreach (var period in periods)
                {
                    await _valuationService.StoreAccountAssetClassValuation(portfolio.Id, account.Id, accByClass, date, period, ct);
                }
            }

            // Portfolio Asset Class
            var portByClass = await _valuationService.GeneratePortfolioAssetClassValuation(
                portfolio.Id, date, reportingCurrency, ct);

            foreach (var period in periods)
            {
                await _valuationService.StorePortfolioAssetClassValuation(portfolio.Id, portByClass, date, period, ct);
            }

            // accumulate owner and estate aggregates
            estateTotal += portVal.TotalValue.Amount;
            estateCash += portVal.CashValue?.Amount ?? 0m;
            estateIncome += portVal.IncomeForDay?.Amount ?? 0m;

            // Estate asset-class
            foreach (var cls in portByClass)
            {
                var key = cls.AssetClass!;
                if (estateClassTotals.TryGetValue(key, out var existing))
                    estateClassTotals[key] = new Money(existing.Amount + cls.TotalValue.Amount, reportingCurrency);
                else
                    estateClassTotals[key] = cls.TotalValue;
            }

            // owner accumulation (assume portfolio.OwnerId exists)
            if (!string.IsNullOrWhiteSpace(portfolio.Owner))
            {
                var owner = portfolio.Owner.Trim();

                if (!ownerTotals.ContainsKey(owner))
                    ownerTotals[owner] = (0m, 0m, 0m);

                ownerTotals[owner] = (
                    ownerTotals[owner].total + portVal.TotalValue.Amount,
                    ownerTotals[owner].cash + (portVal.CashValue?.Amount ?? 0m),
                    ownerTotals[owner].income + (portVal.IncomeForDay?.Amount ?? 0m)
                );

                if (!ownerByClass.ContainsKey(owner))
                    ownerByClass[owner] = new Dictionary<AssetClass, Money>();

                foreach (var cls in portByClass)
                {
                    var c = cls.AssetClass!;
                    if (ownerByClass[owner].TryGetValue(c, out var e))
                        ownerByClass[owner][c] = new Money(e.Amount + cls.TotalValue.Amount, reportingCurrency);
                    else
                        ownerByClass[owner][c] = cls.TotalValue;
                }
            }
        }

        // PHASE 2 — OWNERS
        var owners = portfolios
            .Select(p => p.Owner?.Trim())
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var owner in owners)
        {
            if (String.IsNullOrEmpty(owner)) continue;
            var totals = ownerTotals[owner];

            foreach (var period in periods)
            {
                // 1) SINGLE owner-level snapshot
                var ownerValuation = GenerateAggregateValuation(
                    totals.total,
                    totals.cash,
                    totals.income,
                    reportingCurrency);

                await _valuationService.StoreOwnerValuation(owner, ownerValuation, date, period, ct);

                // 2) ASSET CLASS snapshots
                if (ownerByClass.TryGetValue(owner, out var cls))
                {
                    var snapList = GenerateAggregateAssetClassValuations(
                        reportingCurrency,
                        cls,
                        totals.total);

                    await _valuationService.StoreOwnerAssetClassValuation(owner, snapList, date, period, ct);
                }
            }
        }

        // PHASE 3 — ESTATE
        foreach (var period in periods)
        {
            // 1) ESTATE snapshot
            var estateValuation = GenerateAggregateValuation(
                estateTotal,
                estateCash,
                estateIncome,
                reportingCurrency);

            await _valuationService.StoreEstateValuation(estateValuation, date, period, ct);

            // 2) ESTATE ASSET-CLASS snapshots
            var estateClassSnaps = GenerateAggregateAssetClassValuations(
                reportingCurrency,
                estateClassTotals,
                estateTotal);

            await _valuationService.StoreEstateAssetClassValuation(estateClassSnaps, date, period, ct);
        }
    }
}