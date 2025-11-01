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
        private readonly IPricingService _pricingService;   // Service for calculating account/portfolio values
        private readonly ICashFlowService _cashFlowService; // Service for retrieving deposits, withdrawals, fees

        public PerformanceService(IPricingService pricingService, ICashFlowService cashFlowService)
        {
            _pricingService = pricingService;
            _cashFlowService = cashFlowService;
        }

        // -------------------------
        // DAILY TWR (Account)
        // -------------------------
        /// <summary>
        /// Computes the daily time-weighted return (TWR) for a single account.
        /// Returns a list of DailyReturn objects, one per day.
        /// </summary>
        public async Task<List<DailyReturn>> GetAccountDailyTwrAsync(
            Account account,
            DateTime start,
            DateTime end,
            Currency ccy,
            CancellationToken ct = default)
        {
            // Step 1: Retrieve all relevant cash flows (deposits, withdrawals, fees) for the account
            var flows = await _cashFlowService.GetCashFlowsAsync(account, start, end, ct);

            // Step 2: Aggregate cash flows by day
            var byDayFlows = flows
                .Where(f => f.Amount.Currency == ccy) // Filter to target currency
                .Where(f => f.Type is CashFlowType.Deposit or CashFlowType.Withdrawal or CashFlowType.Fee)
                .GroupBy(f => f.Date.Date) // Group by date (ignore time)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(f => (f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee)
                                    ? -f.Amount.Amount   // Withdrawals and fees reduce value
                                    : f.Amount.Amount)   // Deposits increase value
                );

            var results = new List<DailyReturn>();

            // Step 3: Loop through each day in the period (excluding the start day)
            for (var d = start.Date.AddDays(1); d <= end.Date; d = d.AddDays(1))
            {
                var prev = d.AddDays(-1);

                // Get account value at the start and end of the day
                var moneyStart = await _pricingService.CalculateAccountValueAsync(account, prev, ccy, ct);
                var moneyEnd = await _pricingService.CalculateAccountValueAsync(account, d, ccy, ct);
                var mvStart = moneyStart.Amount;
                var mvEnd = moneyEnd.Amount;

                // Get total cash flows for this day
                var flowsForDay = byDayFlows.TryGetValue(d, out var f) ? f : 0m;

                // Calculate daily TWR: (Ending Value - Starting Value - Flows) / Starting Value
                var r = mvStart == 0m ? 0m : (mvEnd - mvStart - flowsForDay) / mvStart;

                // Add daily return record to results
                results.Add(new DailyReturn(d, EntityKind.Account, account.Id, ccy, r));
            }

            return results;
        }

        // -------------------------
        // DAILY TWR (Portfolio)
        // -------------------------
        /// <summary>
        /// Computes daily TWR for an entire portfolio by aggregating all account flows.
        /// </summary>
        public async Task<List<DailyReturn>> GetPortfolioDailyTwrAsync(
            Portfolio portfolio,
            DateTime start,
            DateTime end,
            Currency ccy,
            CancellationToken ct = default)
        {
            var flowsByDay = new Dictionary<DateTime, decimal>();

            // Step 1: Aggregate cash flows from all accounts
            foreach (var acct in portfolio.Accounts)
            {
                var acctFlows = await _cashFlowService.GetCashFlowsAsync(acct, start, end, ct);

                var grouped = acctFlows
                    .Where(f => f.Amount.Currency == ccy)
                    .Where(f => f.Type is CashFlowType.Deposit or CashFlowType.Withdrawal or CashFlowType.Fee)
                    .GroupBy(f => f.Date.Date)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(f => (f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee)
                                        ? -f.Amount.Amount
                                        : f.Amount.Amount)
                    );

                // Merge account cash flows into portfolio-level flows
                foreach (var kv in grouped)
                    flowsByDay[kv.Key] = flowsByDay.TryGetValue(kv.Key, out var v) ? v + kv.Value : kv.Value;
            }

            var results = new List<DailyReturn>();

            // Step 2: Loop through each day to compute daily portfolio TWR
            for (var d = start.Date.AddDays(1); d <= end.Date; d = d.AddDays(1))
            {
                var prev = d.AddDays(-1);

                // Get portfolio value at the start and end of the day
                var moneyStart = await _pricingService.CalculatePortfolioValueAsync(portfolio, prev, ccy, ct);
                var moneyEnd = await _pricingService.CalculatePortfolioValueAsync(portfolio, d, ccy, ct);
                var mvStart = moneyStart.Amount;
                var mvEnd = moneyEnd.Amount;

                // Get total portfolio flows for this day
                var flows = flowsByDay.TryGetValue(d, out var f) ? f : 0m;

                // Calculate daily TWR
                var r = mvStart == 0m ? 0m : (mvEnd - mvStart - flows) / mvStart;

                results.Add(new DailyReturn(d, EntityKind.Portfolio, portfolio.Id, ccy, r));
            }

            return results;
        }

        // -------------------------
        // LINKING (geometric)
        // -------------------------
        /// <summary>
        /// Combines multiple daily returns into a single cumulative return.
        /// Uses geometric linking: (1 + r1) * (1 + r2) * ... - 1
        /// </summary>
        public decimal Link(IEnumerable<decimal> dailyReturns)
            => dailyReturns.Aggregate(1m, (acc, r) => acc * (1m + r)) - 1m;

        // -------------------------
        // MODIFIED DIETZ (Account)
        // -------------------------
        /// <summary>
        /// Computes Modified Dietz return for a single account over a period.
        /// </summary>
        public async Task<PeriodPerformance> GetAccountReturnAsync(
            Account account,
            DateTime start,
            DateTime end,
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
        /// <summary>
        /// Computes Modified Dietz return for an entire portfolio.
        /// </summary>
        public async Task<PeriodPerformance> GetPortfolioReturnAsync(
            Portfolio portfolio,
            DateTime start,
            DateTime end,
            Currency reportingCurrency,
            CancellationToken ct = default)
        {
            var (B, E, netFlows, weightedFlows) = await GetInputsForDietzAsync(portfolio, start, end, reportingCurrency, ct);
            var r = ComputeModifiedDietz(B.Amount, E.Amount, netFlows.Amount, weightedFlows);
            return new PeriodPerformance(start, end, reportingCurrency, ReturnMethod.ModifiedDietz, r, B, E, netFlows);
        }

        // -------------------------
        // Helper Methods
        // -------------------------

        /// <summary>
        /// Computes the Modified Dietz return from beginning value, ending value,
        /// net cash flows, and weighted flows.
        /// Formula: (E - B - netFlows) / (B + weightedFlows)
        /// </summary>
        private static decimal ComputeModifiedDietz(decimal B, decimal E, decimal netFlows, decimal weightedFlows)
        {
            var numerator = E - B - netFlows;
            var denominator = B + weightedFlows;
            return denominator == 0m ? 0m : numerator / denominator;
        }

        /// <summary>
        /// Retrieves inputs required for Modified Dietz calculation for a single account.
        /// Computes beginning/ending values, net cash flows, and time-weighted flows.
        /// </summary>
        private async Task<(Money B, Money E, Money netFlows, decimal weightedFlows)> GetInputsForDietzAsync(
            Account account, DateTime start, DateTime end, Currency ccy, CancellationToken ct = default)
        {
            // Get account value at start and end of period
            var B = await _pricingService.CalculateAccountValueAsync(account, start, ccy, ct);
            var E = await _pricingService.CalculateAccountValueAsync(account, end, ccy, ct);

            // Get relevant cash flows
            var flows = await _cashFlowService.GetCashFlowsAsync(account, start, end, ct);
            var relevant = flows
                .Where(f => f.Amount.Currency == ccy)
                .Where(f => f.Type is CashFlowType.Deposit or CashFlowType.Withdrawal or CashFlowType.Fee)
                .OrderBy(f => f.Date)
                .ToList();

            // Compute net and time-weighted flows
            var totalDays = Math.Max(1, (end.Date - start.Date).Days);
            decimal net = 0m, weighted = 0m;

            foreach (var f in relevant)
            {
                var signed = (f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee)
                    ? -f.Amount.Amount
                    : f.Amount.Amount;

                net += signed;

                // Weight flow based on time in period
                var t = (decimal)(f.Date.Date - start.Date).Days / totalDays; // 0..1
                weighted += signed * (1m - t);
            }

            return (B, E, new Money(net, ccy), weighted);
        }

        /// <summary>
        /// Retrieves inputs required for Modified Dietz calculation for a portfolio.
        /// Aggregates all account cash flows.
        /// </summary>
        private async Task<(Money B, Money E, Money netFlows, decimal weightedFlows)> GetInputsForDietzAsync(
            Portfolio portfolio, DateTime start, DateTime end, Currency ccy, CancellationToken ct = default)
        {
            var B = await _pricingService.CalculatePortfolioValueAsync(portfolio, start, ccy, ct);
            var E = await _pricingService.CalculatePortfolioValueAsync(portfolio, end, ccy, ct);

            var totalDays = Math.Max(1, (end.Date - start.Date).Days);
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
                    var t = (decimal)(f.Date.Date - start.Date).Days / totalDays;
                    weighted += signed * (1m - t);
                }
            }

            return (B, E, new Money(net, ccy), weighted);
        }
    }
}
