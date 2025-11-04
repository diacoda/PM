using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Services
{
    /// <summary>
    /// Service for calculating portfolio and account performance, including
    /// daily TWR (Time-Weighted Return) and Modified Dietz returns.
    /// </summary>
    public class PerformanceService : IPerformanceService
    {
        private readonly IPricingService _pricingService;
        private readonly ICashFlowService _cashFlowService;

        public PerformanceService(IPricingService pricingService, ICashFlowService cashFlowService)
        {
            _pricingService = pricingService;
            _cashFlowService = cashFlowService;
        }

        // -------------------------
        // DAILY TWR (Account)
        // -------------------------
        public async Task<List<DailyReturn>> GetAccountDailyTwrAsync(
            Account account,
            DateOnly start,
            DateOnly end,
            Currency ccy,
            CancellationToken ct = default)
        {
            var flows = await _cashFlowService.GetCashFlowsAsync(account, start, end, ct);

            var byDayFlows = flows
                .Where(f => f.Amount.Currency == ccy)
                .Where(f => f.Type is CashFlowType.Deposit or CashFlowType.Withdrawal or CashFlowType.Fee)
                .GroupBy(f => f.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(f => (f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee)
                        ? -f.Amount.Amount
                        : f.Amount.Amount)
                );

            var results = new List<DailyReturn>();

            for (var d = start.AddDays(1); d <= end; d = d.AddDays(1))
            {
                var prev = d.AddDays(-1);

                var moneyStart = await _pricingService.CalculateAccountValueAsync(account, prev, ccy, ct);
                var moneyEnd = await _pricingService.CalculateAccountValueAsync(account, d, ccy, ct);

                var mvStart = moneyStart.Amount;
                var mvEnd = moneyEnd.Amount;

                var flowsForDay = byDayFlows.TryGetValue(d, out var f) ? f : 0m;

                var r = mvStart == 0m ? 0m : (mvEnd - mvStart - flowsForDay) / mvStart;

                results.Add(new DailyReturn(d.ToDateTime(new TimeOnly(0,0,0)), EntityKind.Account, account.Id, ccy, r));
            }

            return results;
        }

        // -------------------------
        // DAILY TWR (Portfolio)
        // -------------------------
        public async Task<List<DailyReturn>> GetPortfolioDailyTwrAsync(
            Portfolio portfolio,
            DateOnly start,
            DateOnly end,
            Currency ccy,
            CancellationToken ct = default)
        {
            var flowsByDay = new Dictionary<DateOnly, decimal>();

            foreach (var acct in portfolio.Accounts)
            {
                var acctFlows = await _cashFlowService.GetCashFlowsAsync(acct, start, end, ct);

                var grouped = acctFlows
                    .Where(f => f.Amount.Currency == ccy)
                    .Where(f => f.Type is CashFlowType.Deposit or CashFlowType.Withdrawal or CashFlowType.Fee)
                    .GroupBy(f => f.Date)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(f => (f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee)
                            ? -f.Amount.Amount
                            : f.Amount.Amount)
                    );

                foreach (var kv in grouped)
                    flowsByDay[kv.Key] = flowsByDay.TryGetValue(kv.Key, out var v) ? v + kv.Value : kv.Value;
            }

            var results = new List<DailyReturn>();

            for (var d = start.AddDays(1); d <= end; d = d.AddDays(1))
            {
                var prev = d.AddDays(-1);

                var moneyStart = await _pricingService.CalculatePortfolioValueAsync(portfolio, prev, ccy, ct);
                var moneyEnd = await _pricingService.CalculatePortfolioValueAsync(portfolio, d, ccy, ct);

                var mvStart = moneyStart.Amount;
                var mvEnd = moneyEnd.Amount;

                var flows = flowsByDay.TryGetValue(d, out var f) ? f : 0m;

                var r = mvStart == 0m ? 0m : (mvEnd - mvStart - flows) / mvStart;

                results.Add(new DailyReturn(d.ToDateTime(new TimeOnly(0,0,0)), EntityKind.Portfolio, portfolio.Id, ccy, r));
            }

            return results;
        }

        // -------------------------
        // LINKING (geometric)
        // -------------------------
        public decimal Link(IEnumerable<decimal> dailyReturns)
            => dailyReturns.Aggregate(1m, (acc, r) => acc * (1m + r)) - 1m;

        // -------------------------
        // MODIFIED DIETZ (Account)
        // -------------------------
        public async Task<PeriodPerformance> GetAccountReturnAsync(
            Account account,
            DateOnly start,
            DateOnly end,
            Currency reportingCurrency,
            CancellationToken ct = default)
        {
            var (B, E, netFlows, weightedFlows) = await GetInputsForDietzAsync(account, start, end, reportingCurrency, ct);
            var r = ComputeModifiedDietz(B.Amount, E.Amount, netFlows.Amount, weightedFlows);
            return new PeriodPerformance(start, end, reportingCurrency, ReturnMethod.ModifiedDietz, r, B, E, netFlows);
        }

        // -------------------------
        // MODIFIED DIETZ (Portfolio)
        // -------------------------
        public async Task<PeriodPerformance> GetPortfolioReturnAsync(
            Portfolio portfolio,
            DateOnly start,
            DateOnly end,
            Currency reportingCurrency,
            CancellationToken ct = default)
        {
            var (B, E, netFlows, weightedFlows) = await GetInputsForDietzAsync(portfolio, start, end, reportingCurrency, ct);
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

        private async Task<(Money B, Money E, Money netFlows, decimal weightedFlows)> GetInputsForDietzAsync(
            Account account, DateOnly start, DateOnly end, Currency ccy, CancellationToken ct = default)
        {
            var B = await _pricingService.CalculateAccountValueAsync(account, start, ccy, ct);
            var E = await _pricingService.CalculateAccountValueAsync(account, end, ccy, ct);

            var flows = await _cashFlowService.GetCashFlowsAsync(account, start, end, ct);
            var relevant = flows
                .Where(f => f.Amount.Currency == ccy)
                .Where(f => f.Type is CashFlowType.Deposit or CashFlowType.Withdrawal or CashFlowType.Fee)
                .OrderBy(f => f.Date)
                .ToList();

            var totalDays = Math.Max(1, (end.DayNumber - start.DayNumber));
            decimal net = 0m, weighted = 0m;

            foreach (var f in relevant)
            {
                var signed = (f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee)
                    ? -f.Amount.Amount
                    : f.Amount.Amount;

                net += signed;

                var t = (decimal)(f.Date.DayNumber - start.DayNumber) / totalDays;
                weighted += signed * (1m - t);
            }

            return (B, E, new Money(net, ccy), weighted);
        }

        private async Task<(Money B, Money E, Money netFlows, decimal weightedFlows)> GetInputsForDietzAsync(
            Portfolio portfolio, DateOnly start, DateOnly end, Currency ccy, CancellationToken ct = default)
        {
            var B = await _pricingService.CalculatePortfolioValueAsync(portfolio, start, ccy, ct);
            var E = await _pricingService.CalculatePortfolioValueAsync(portfolio, end, ccy, ct);

            var totalDays = Math.Max(1, (end.DayNumber - start.DayNumber));
            decimal net = 0m, weighted = 0m;

            foreach (var acct in portfolio.Accounts)
            {
                var flows = await _cashFlowService.GetCashFlowsAsync(acct, start, end, ct);
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

                    var t = (decimal)(f.Date.DayNumber - start.DayNumber) / totalDays;
                    weighted += signed * (1m - t);
                }
            }

            return (B, E, new Money(net, ccy), weighted);
        }
    }
}
