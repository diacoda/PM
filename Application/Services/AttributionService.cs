using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Services;

public class AttributionService : IAttributionService
{
    private readonly IPricingService _pricingService;

    public AttributionService(IPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    // SECURITY-LEVEL for an Account
    public async Task<IEnumerable<ContributionRecord>> ContributionBySecurityAsync(
        Account account, DateTime start, DateTime end, Currency ccy, CancellationToken ct = default)
    {
        var results = new List<ContributionRecord>();

        var moneyStart = await _pricingService.CalculateAccountValueAsync(account, start, ccy, ct);
        var accountStart = moneyStart.Amount;
        if (accountStart == 0m)
            return results;

        foreach (var h in account.Holdings)
        {
            var v0Money = await _pricingService.CalculateHoldingValueAsync(h, start, ccy, ct);
            var v0 = v0Money.Amount;
            var v1Money = await _pricingService.CalculateHoldingValueAsync(h, end, ccy, ct);
            var v1 = v1Money.Amount;

            if (v0 <= 0m) continue;

            var r = (v1 - v0) / v0;
            var w = v0 / accountStart;

            results.Add(new ContributionRecord(
                start, end, ccy, ContributionLevel.Security, h.Symbol.Code, w, r, w * r
            ));
        }

        return results;
    }


    // SECURITY-LEVEL for a Portfolio (sum across accounts)
    public async Task<IEnumerable<ContributionRecord>> ContributionBySecurityAsync(
        Portfolio portfolio, DateTime start, DateTime end, Currency ccy, CancellationToken ct = default)
    {
        var results = new List<ContributionRecord>();

        var p0Money = await _pricingService.CalculatePortfolioValueAsync(portfolio, start, ccy, ct);
        var p0 = p0Money.Amount;
        if (p0 == 0m)
            return results;

        var groupedHoldings = portfolio.Accounts
            .SelectMany(a => a.Holdings)
            .GroupBy(h => h.Symbol.Code);

        foreach (var g in groupedHoldings)
        {
            decimal v0 = 0m, v1 = 0m;

            foreach (var hh in g)
            {
                var tmpV0Money = await _pricingService.CalculateHoldingValueAsync(hh, start, ccy, ct);
                v0 += tmpV0Money.Amount;

                var tmpV1Money = await _pricingService.CalculateHoldingValueAsync(hh, end, ccy, ct);
                v1 += tmpV1Money.Amount;
            }

            if (v0 <= 0m)
                continue;

            var r = (v1 - v0) / v0;
            var w = v0 / p0;

            results.Add(new ContributionRecord(
                start, end, ccy,
                ContributionLevel.Security,
                g.Key, w, r, w * r
            ));
        }

        return results;
    }


    // ASSET CLASS aggregation for Portfolio
    public async Task<IEnumerable<ContributionRecord>> ContributionByAssetClassAsync(
        Portfolio portfolio, DateTime start, DateTime end, Currency ccy, CancellationToken ct = default)
    {
        var securities = await ContributionBySecurityAsync(portfolio, start, end, ccy, ct);

        return securities
            .GroupBy(c => SymbolToAssetClass(c.Key, portfolio))
            .Select(g =>
            {
                var startWeight = g.Sum(x => x.StartWeight);
                var contrib = g.Sum(x => x.Contribution);
                var ret = startWeight == 0m ? 0m : contrib / startWeight;

                return new ContributionRecord(
                    start, end, ccy,
                    ContributionLevel.AssetClass,
                    g.Key, startWeight, ret, contrib
                );
            });
    }


    private static string SymbolToAssetClass(string symbol, Portfolio portfolio)
    {
        foreach (var a in portfolio.Accounts)
        {
            var h = a.Holdings.FirstOrDefault(hh => hh.Symbol.Code == symbol);
            if (h != null) return h.Symbol.AssetClass.ToString();
        }
        return "Other";
    }
}