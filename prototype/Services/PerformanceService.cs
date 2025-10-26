using model.Domain.Entities;
using model.Domain.Values;

namespace model.Services;

public class PerformanceService
{
    private readonly ValuationService _valuationService;
    private readonly CashFlowService _cashFlowService;

    public PerformanceService(ValuationService valuationService, CashFlowService cashFlowService)
    {
        _valuationService = valuationService;
        _cashFlowService = cashFlowService;
    }

    // -------------------------
    // DAILY TWR (Account)
    // Convention: external flows occur at END of day.
    // r_d = (MV_end - MV_start - Flows_d) / MV_start
    // Only Deposit/Withdrawal/Fee count as flows.
    // -------------------------
    public IEnumerable<DailyReturn> GetAccountDailyTwr(Account account, DateTime start, DateTime end, Currency ccy)
    {
        var byDayFlows = _cashFlowService
            .GetCashFlows(account, start, end)
            .Where(f => f.Amount.Currency == ccy)
            .Where(f => f.Type is CashFlowType.Deposit or CashFlowType.Withdrawal or CashFlowType.Fee) // ignore internal events
            .GroupBy(f => f.Date.Date)
            .ToDictionary(g => g.Key, g => g.Sum(f =>
                (f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee) ? -f.Amount.Amount : f.Amount.Amount));

        for (var d = start.Date.AddDays(1); d <= end.Date; d = d.AddDays(1))
        {
            var prev = d.AddDays(-1);
            var mvStart = _valuationService.CalculateAccountValue(account, prev, ccy).Amount;
            var mvEnd   = _valuationService.CalculateAccountValue(account, d,    ccy).Amount;
            var flows   = byDayFlows.TryGetValue(d, out var f) ? f : 0m;

            var r = mvStart == 0m ? 0m : (mvEnd - mvStart - flows) / mvStart;
            yield return new DailyReturn(d, EntityKind.Account, account.Id, ccy, r);
        }
    }

    // -------------------------
    // DAILY TWR (Portfolio)
    // -------------------------
    public IEnumerable<DailyReturn> GetPortfolioDailyTwr(Portfolio portfolio, DateTime start, DateTime end, Currency ccy)
    {
        var flowsByDay = new Dictionary<DateTime, decimal>();

        foreach (var acct in portfolio.Accounts)
        {
            var acctFlows = _cashFlowService
                .GetCashFlows(acct, start, end)
                .Where(f => f.Amount.Currency == ccy)
                .Where(f => f.Type is CashFlowType.Deposit or CashFlowType.Withdrawal or CashFlowType.Fee) // ignore internal events
                .GroupBy(f => f.Date.Date)
                .ToDictionary(g => g.Key, g => g.Sum(f =>
                    (f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee) ? -f.Amount.Amount : f.Amount.Amount));

            foreach (var kv in acctFlows)
                flowsByDay[kv.Key] = flowsByDay.TryGetValue(kv.Key, out var v) ? v + kv.Value : kv.Value;
        }

        for (var d = start.Date.AddDays(1); d <= end.Date; d = d.AddDays(1))
        {
            var prev = d.AddDays(-1);
            var mvStart = _valuationService.CalculatePortfolioValue(portfolio, prev, ccy).Amount;
            var mvEnd   = _valuationService.CalculatePortfolioValue(portfolio, d,    ccy).Amount;
            var flows   = flowsByDay.TryGetValue(d, out var f) ? f : 0m;

            var r = mvStart == 0m ? 0m : (mvEnd - mvStart - flows) / mvStart;
            yield return new DailyReturn(d, EntityKind.Portfolio, portfolio.Id, ccy, r);
        }
    }

    // -------------------------
    // LINKING (geometric)
    // -------------------------
    public decimal Link(IEnumerable<decimal> dailyReturns)
        => dailyReturns.Aggregate(1m, (acc, r) => acc * (1m + r)) - 1m;

    // -------------------------
    // MODIFIED DIETZ (period)
    // -------------------------
    public PeriodPerformance GetAccountReturn(Account account, DateTime start, DateTime end, Currency reportingCurrency)
    {
        var (B, E, netFlows, weightedFlows) = GetInputsForDietz(account, start, end, reportingCurrency);
        var r = ComputeModifiedDietz(B.Amount, E.Amount, netFlows.Amount, weightedFlows);
        return new PeriodPerformance(start, end, reportingCurrency, ReturnMethod.ModifiedDietz, r, B, E, netFlows);
    }

    public PeriodPerformance GetPortfolioReturn(Portfolio portfolio, DateTime start, DateTime end, Currency reportingCurrency)
    {
        var (B, E, netFlows, weightedFlows) = GetInputsForDietz(portfolio, start, end, reportingCurrency);
        var r = ComputeModifiedDietz(B.Amount, E.Amount, netFlows.Amount, weightedFlows);
        return new PeriodPerformance(start, end, reportingCurrency, ReturnMethod.ModifiedDietz, r, B, E, netFlows);
    }

    private static decimal ComputeModifiedDietz(decimal B, decimal E, decimal netFlows, decimal weightedFlows)
    {
        var numerator = E - B - netFlows;
        var denominator = B + weightedFlows;
        return denominator == 0m ? 0m : numerator / denominator;
    }

    private (Money B, Money E, Money netFlows, decimal weightedFlows)
        GetInputsForDietz(Account account, DateTime start, DateTime end, Currency ccy)
    {
        var B = _valuationService.CalculateAccountValue(account, start, ccy);
        var E = _valuationService.CalculateAccountValue(account, end, ccy);

        var flows = _cashFlowService.GetCashFlows(account, start, end)
                    .Where(f => f.Amount.Currency == ccy)
                    .Where(f => f.Type is CashFlowType.Deposit or CashFlowType.Withdrawal or CashFlowType.Fee)
                    .OrderBy(f => f.Date)
                    .ToList();

        var totalDays = Math.Max(1, (end.Date - start.Date).Days);
        decimal net = 0m, weighted = 0m;

        foreach (var f in flows)
        {
            var signed = (f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee) ? -f.Amount.Amount : f.Amount.Amount;
            net += signed;
            var t = (decimal)(f.Date.Date - start.Date).Days / totalDays; // 0..1
            weighted += signed * (1m - t);
        }

        return (B, E, new Money(net, ccy), weighted);
    }

    private (Money B, Money E, Money netFlows, decimal weightedFlows)
        GetInputsForDietz(Portfolio portfolio, DateTime start, DateTime end, Currency ccy)
    {
        var B = _valuationService.CalculatePortfolioValue(portfolio, start, ccy);
        var E = _valuationService.CalculatePortfolioValue(portfolio, end, ccy);

        var totalDays = Math.Max(1, (end.Date - start.Date).Days);
        decimal net = 0m, weighted = 0m;

        foreach (var acct in portfolio.Accounts)
        {
            var flows = _cashFlowService.GetCashFlows(acct, start, end)
                        .Where(f => f.Amount.Currency == ccy)
                        .Where(f => f.Type is CashFlowType.Deposit or CashFlowType.Withdrawal or CashFlowType.Fee)
                        .OrderBy(f => f.Date);

            foreach (var f in flows)
            {
                var signed = (f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee) ? -f.Amount.Amount : f.Amount.Amount;
                net += signed;
                var t = (decimal)(f.Date.Date - start.Date).Days / totalDays;
                weighted += signed * (1m - t);
            }
        }

        return (B, E, new Money(net, ccy), weighted);
    }
}
