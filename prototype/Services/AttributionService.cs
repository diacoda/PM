using model.Domain.Entities;
using model.Domain.Values;

namespace model.Services;

public class AttributionService
{
    private readonly ValuationService _valuationService;

    public AttributionService(ValuationService valuationService)
    {
        _valuationService = valuationService;
    }

    // SECURITY-LEVEL for an Account
    public IEnumerable<ContributionRecord> ContributionBySecurity(Account account, DateTime start, DateTime end, Currency ccy)
    {
        var accountStart = _valuationService.CalculateAccountValue(account, start, ccy).Amount;
        if (accountStart == 0m) yield break;

        foreach (var h in account.Holdings)
        {
            var v0 = _valuationService.CalculateHoldingValue(h, start, ccy).Amount;
            var v1 = _valuationService.CalculateHoldingValue(h, end,   ccy).Amount;
            if (v0 <= 0m) continue;

            var r = (v1 - v0) / v0;
            var w = v0 / accountStart;
            yield return new ContributionRecord(
                start, end, ccy, ContributionLevel.Security, h.Instrument.Symbol.Code, w, r, w * r
            );
        }
    }

    // SECURITY-LEVEL for a Portfolio (sum across accounts)
    public IEnumerable<ContributionRecord> ContributionBySecurity(Portfolio portfolio, DateTime start, DateTime end, Currency ccy)
    {
        var p0 = _valuationService.CalculatePortfolioValue(portfolio, start, ccy).Amount;
        if (p0 == 0m) yield break;

        var all = portfolio.Accounts.SelectMany(a => a.Holdings).GroupBy(h => h.Instrument.Symbol.Code);
        foreach (var g in all)
        {
            decimal v0 = 0m, v1 = 0m;
            foreach (var hh in g)
            {
                v0 += _valuationService.CalculateHoldingValue(hh, start, ccy).Amount;
                v1 += _valuationService.CalculateHoldingValue(hh, end,   ccy).Amount;
            }
            if (v0 <= 0m) continue;

            var r = (v1 - v0) / v0;
            var w = v0 / p0;
            yield return new ContributionRecord(start, end, ccy, ContributionLevel.Security, g.Key, w, r, w * r);
        }
    }

    // ASSET CLASS aggregation for Portfolio
    public IEnumerable<ContributionRecord> ContributionByAssetClass(Portfolio portfolio, DateTime start, DateTime end, Currency ccy)
        => ContributionBySecurity(portfolio, start, end, ccy)
           .GroupBy(c => SymbolToAssetClass(c.Key, portfolio))
           .Select(g =>
           {
               var startWeight = g.Sum(x => x.StartWeight);
               var contrib = g.Sum(x => x.Contribution);
               var ret = startWeight == 0m ? 0m : contrib / startWeight;
               return new ContributionRecord(start, end, ccy, ContributionLevel.AssetClass, g.Key, startWeight, ret, contrib);
           });

    private static string SymbolToAssetClass(string symbol, Portfolio portfolio)
    {
        foreach (var a in portfolio.Accounts)
        {
            var h = a.Holdings.FirstOrDefault(hh => hh.Instrument.Symbol.Code == symbol);
            if (h != null) return h.Instrument.AssetClass.ToString();
        }
        return "Other";
    }
}