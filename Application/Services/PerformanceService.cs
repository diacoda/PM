using System.Threading.Tasks;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Services;

public class PerformanceService : IPerformanceService
{
    private readonly IPricingService _valuationService;
    private readonly ICashFlowService _cashFlowService;

    public PerformanceService(IPricingService valuationService, ICashFlowService cashFlowService)
    {
        _valuationService = valuationService;
        _cashFlowService = cashFlowService;
    }

    // -------------------------
    // DAILY TWR (Account)
    // -------------------------
    public async IAsyncEnumerable<DailyReturn> GetAccountDailyTwrAsync(Account account, DateTime start, DateTime end, Currency ccy)
    {
        var flows = await _cashFlowService.GetCashFlowsAsync(account, start, end);

        var byDayFlows = flows
            .Where(f => f.Amount.Currency == ccy)
            .Where(f => f.Type is CashFlowType.Deposit or CashFlowType.Withdrawal or CashFlowType.Fee)
            .GroupBy(f => f.Date.Date)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(f =>
                    (f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee)
                        ? -f.Amount.Amount
                        : f.Amount.Amount)
            );

        for (var d = start.Date.AddDays(1); d <= end.Date; d = d.AddDays(1))
        {
            var prev = d.AddDays(-1);
            var mvStart = _valuationService.CalculateAccountValue(account, prev, ccy).Amount;
            var mvEnd = _valuationService.CalculateAccountValue(account, d, ccy).Amount;
            var flowsForDay = byDayFlows.TryGetValue(d, out var f) ? f : 0m;

            var r = mvStart == 0m ? 0m : (mvEnd - mvStart - flowsForDay) / mvStart;
            yield return new DailyReturn(d, EntityKind.Account, account.Id, ccy, r);
        }
    }

    // -------------------------
    // DAILY TWR (Portfolio)
    // -------------------------
    public async IAsyncEnumerable<DailyReturn> GetPortfolioDailyTwrAsync(Portfolio portfolio, DateTime start, DateTime end, Currency ccy)
    {
        var flowsByDay = new Dictionary<DateTime, decimal>();

        foreach (var acct in portfolio.Accounts)
        {
            var acctFlows = await _cashFlowService.GetCashFlowsAsync(acct, start, end);

            var grouped = acctFlows
                .Where(f => f.Amount.Currency == ccy)
                .Where(f => f.Type is CashFlowType.Deposit or CashFlowType.Withdrawal or CashFlowType.Fee)
                .GroupBy(f => f.Date.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(f =>
                        (f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee)
                            ? -f.Amount.Amount
                            : f.Amount.Amount)
                );

            foreach (var kv in grouped)
                flowsByDay[kv.Key] = flowsByDay.TryGetValue(kv.Key, out var v) ? v + kv.Value : kv.Value;
        }

        for (var d = start.Date.AddDays(1); d <= end.Date; d = d.AddDays(1))
        {
            var prev = d.AddDays(-1);
            var mvStart = _valuationService.CalculatePortfolioValue(portfolio, prev, ccy).Amount;
            var mvEnd = _valuationService.CalculatePortfolioValue(portfolio, d, ccy).Amount;
            var flows = flowsByDay.TryGetValue(d, out var f) ? f : 0m;

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
    // MODIFIED DIETZ (Account)
    // -------------------------
    public async Task<PeriodPerformance> GetAccountReturnAsync(Account account, DateTime start, DateTime end, Currency reportingCurrency)
    {
        var (B, E, netFlows, weightedFlows) = await GetInputsForDietzAsync(account, start, end, reportingCurrency);
        var r = ComputeModifiedDietz(B.Amount, E.Amount, netFlows.Amount, weightedFlows);
        return new PeriodPerformance(start, end, reportingCurrency, ReturnMethod.ModifiedDietz, r, B, E, netFlows);
    }

    // -------------------------
    // MODIFIED DIETZ (Portfolio)
    // -------------------------
    public async Task<PeriodPerformance> GetPortfolioReturnAsync(Portfolio portfolio, DateTime start, DateTime end, Currency reportingCurrency)
    {
        var (B, E, netFlows, weightedFlows) = await GetInputsForDietzAsync(portfolio, start, end, reportingCurrency);
        var r = ComputeModifiedDietz(B.Amount, E.Amount, netFlows.Amount, weightedFlows);
        return new PeriodPerformance(start, end, reportingCurrency, ReturnMethod.ModifiedDietz, r, B, E, netFlows);
    }

    // -------------------------
    // Helpers
    // -------------------------
    private static decimal ComputeModifiedDietz(decimal B, decimal E, decimal netFlows, decimal weightedFlows)
    {
        var numerator = E - B - netFlows;
        var denominator = B + weightedFlows;
        return denominator == 0m ? 0m : numerator / denominator;
    }

    private async Task<(Money B, Money E, Money netFlows, decimal weightedFlows)>
        GetInputsForDietzAsync(Account account, DateTime start, DateTime end, Currency ccy)
    {
        var B = _valuationService.CalculateAccountValue(account, start, ccy);
        var E = _valuationService.CalculateAccountValue(account, end, ccy);

        var flows = await _cashFlowService.GetCashFlowsAsync(account, start, end);
        var relevant = flows
            .Where(f => f.Amount.Currency == ccy)
            .Where(f => f.Type is CashFlowType.Deposit or CashFlowType.Withdrawal or CashFlowType.Fee)
            .OrderBy(f => f.Date)
            .ToList();

        var totalDays = Math.Max(1, (end.Date - start.Date).Days);
        decimal net = 0m, weighted = 0m;

        foreach (var f in relevant)
        {
            var signed = (f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee)
                ? -f.Amount.Amount
                : f.Amount.Amount;

            net += signed;
            var t = (decimal)(f.Date.Date - start.Date).Days / totalDays; // 0..1
            weighted += signed * (1m - t);
        }

        return (B, E, new Money(net, ccy), weighted);
    }

    private async Task<(Money B, Money E, Money netFlows, decimal weightedFlows)>
        GetInputsForDietzAsync(Portfolio portfolio, DateTime start, DateTime end, Currency ccy)
    {
        var B = _valuationService.CalculatePortfolioValue(portfolio, start, ccy);
        var E = _valuationService.CalculatePortfolioValue(portfolio, end, ccy);

        var totalDays = Math.Max(1, (end.Date - start.Date).Days);
        decimal net = 0m, weighted = 0m;

        foreach (var acct in portfolio.Accounts)
        {
            var flows = await _cashFlowService.GetCashFlowsAsync(acct, start, end);
            var relevant = flows
                .Where(f => f.Amount.Currency == ccy)
                .Where(f => f.Type is CashFlowType.Deposit or CashFlowType.Withdrawal or CashFlowType.Fee)
                .OrderBy(f => f.Date);

            foreach (var f in relevant)
            {
                var signed = (f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee)
                    ? -f.Amount.Amount
                    : f.Amount.Amount;

                net += signed;
                var t = (decimal)(f.Date.Date - start.Date).Days / totalDays;
                weighted += signed * (1m - t);
            }
        }

        return (B, E, new Money(net, ccy), weighted);
    }
}
