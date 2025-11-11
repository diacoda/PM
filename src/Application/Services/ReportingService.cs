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
    public class ReportingService : IReportingService
    {
        private readonly IPricingService _pricingService;

        public ReportingService(IPricingService pricingService)
        {
            _pricingService = pricingService;
        }

        // ==================== AGGREGATION BY ASSET CLASS ====================

        public async Task<Dictionary<AssetClass, Money>> AggregateByAssetClassAsync(
            Account account,
            DateOnly date,
            Currency reportingCurrency,
            CancellationToken ct = default)
        {
            var result = new Dictionary<AssetClass, Money>();

            foreach (var holding in account.Holdings)
            {
                var value = await _pricingService.CalculateHoldingValueAsync(holding, date, reportingCurrency, ct);
                var assetClass = holding.Asset.AssetClass;

                if (result.ContainsKey(assetClass))
                {
                    var existing = result[assetClass];
                    result[assetClass] = new Money(existing.Amount + value.Amount, reportingCurrency);
                }
                else
                {
                    result[assetClass] = value;
                }
            }

            return result;
        }

        public async Task<Dictionary<AssetClass, Money>> AggregateByAssetClassAsync(
            Portfolio portfolio,
            DateOnly date,
            Currency reportingCurrency,
            CancellationToken ct = default)
        {
            var result = new Dictionary<AssetClass, Money>();

            foreach (var account in portfolio.Accounts)
            {
                var accountAggregation = await AggregateByAssetClassAsync(account, date, reportingCurrency, ct);

                foreach (var kvp in accountAggregation)
                {
                    if (result.ContainsKey(kvp.Key))
                    {
                        var existing = result[kvp.Key];
                        result[kvp.Key] = new Money(existing.Amount + kvp.Value.Amount, reportingCurrency);
                    }
                    else
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                }
            }

            return result;
        }

        // ==================== HOLDINGS SUMMARY ====================

        public void PrintHoldingsSummary(Account account)
        {
            Console.WriteLine($"Holdings Summary for Account: {account.Name} ({account.Currency})");

            foreach (var holding in account.Holdings)
            {
                Console.WriteLine($"- {holding.Asset} | Asset Class: {holding.Asset.AssetClass} | Quantity: {holding.Quantity}");
            }
        }

        // ==================== TRANSACTION HISTORY ====================

        public void PrintTransactionHistory(Account account, DateOnly from, DateOnly to)
        {
            Console.WriteLine($"Transaction History for Account: {account.Name} from {from:yyyy-MM-dd} to {to:yyyy-MM-dd}");

            var filtered = account.Transactions
                .Where(t => t.Date >= from && t.Date <= to)
                .OrderBy(t => t.Date);

            foreach (var tx in filtered)
            {
                Console.WriteLine($"- {tx.Date:yyyy-MM-dd} | {tx.Type} | Qty: {tx.Quantity} | Amount: {tx.Amount.Amount} {tx.Amount.Currency}");
            }
        }

        public async Task<Dictionary<AssetClass, decimal>> GetAssetClassPercentagesAsync(Account account, DateOnly date, Currency reportingCurrency, CancellationToken ct = default)
        {
            var totals = await AggregateByAssetClassAsync(account, date, reportingCurrency, ct);
            var grand = totals.Values.Sum(m => m.Amount);
            if (grand <= 0m) return new();

            return totals.ToDictionary(k => k.Key, v => v.Value.Amount / grand);
        }

        public async Task<Dictionary<AssetClass, decimal>> GetAssetClassPercentagesAsync(Portfolio portfolio, DateOnly date, Currency reportingCurrency, CancellationToken ct = default)
        {
            var totals = await AggregateByAssetClassAsync(portfolio, date, reportingCurrency, ct);
            var grand = totals.Values.Sum(m => m.Amount);
            if (grand <= 0m) return new();

            return totals.ToDictionary(k => k.Key, v => v.Value.Amount / grand);
        }

        // ==================== TRANSACTION COSTS ====================

        public Dictionary<Currency, decimal> GetTradingCostsByCurrency(Account account, DateOnly from, DateOnly to)
        {
            var relevantTx = account.Transactions
                .Where(t => t.Date >= from && t.Date <= to)
                .Where(t => t.Type == TransactionType.Buy ||
                            t.Type == TransactionType.Sell ||
                            t.Type.IsIncome())
                .Where(t => t.Costs is not null && t.Costs.Amount > 0m);

            return relevantTx
                .GroupBy(t => t.Costs!.Currency)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Costs!.Amount));
        }

        public Dictionary<Currency, decimal> GetTradingCostsByCurrency(Portfolio portfolio, DateOnly from, DateOnly to)
        {
            var agg = new Dictionary<Currency, decimal>();
            foreach (var acct in portfolio.Accounts)
            {
                foreach (var kv in GetTradingCostsByCurrency(acct, from, to))
                    agg[kv.Key] = agg.TryGetValue(kv.Key, out var v) ? v + kv.Value : kv.Value;
            }
            return agg;
        }

        public IEnumerable<TransactionCostSummary> GetTransactionCostSummaries(Account account, DateOnly from, DateOnly to)
        {
            var tx = account.Transactions
                .Where(t => t.Date >= from && t.Date <= to)
                .Where(t => t.Type == TransactionType.Buy ||
                            t.Type == TransactionType.Sell ||
                            t.Type == TransactionType.Dividend ||
                            t.Type == TransactionType.Interest)
                .Where(t => t.Costs is not null && t.Costs.Amount > 0m)
                .ToList();

            return tx
                .GroupBy(t => t.Costs!.Currency)
                .Select(g =>
                {
                    var ccy = g.Key;
                    var buys = g.Where(t => t.Type == TransactionType.Buy).ToList();
                    var sells = g.Where(t => t.Type == TransactionType.Sell).ToList();
                    var dividends = g.Where(t => t.Type == TransactionType.Dividend).ToList();
                    var interests = g.Where(t => t.Type == TransactionType.Interest).ToList();

                    decimal buyCosts = buys.Sum(t => t.Costs!.Amount);
                    decimal sellCosts = sells.Sum(t => t.Costs!.Amount);
                    decimal dividendCosts = dividends.Sum(t => t.Costs!.Amount);
                    decimal interestCosts = interests.Sum(t => t.Costs!.Amount);

                    decimal buyGross = buys.Sum(t => t.Amount.Amount);
                    decimal sellGross = sells.Sum(t => t.Amount.Amount);
                    decimal dividendGross = dividends.Sum(t => t.Amount.Amount);
                    decimal interestGross = interests.Sum(t => t.Amount.Amount);

                    return new TransactionCostSummary(
                        Currency: ccy,
                        TotalCosts: buyCosts + sellCosts + dividendCosts + interestCosts,
                        BuyCount: buys.Count,
                        SellCount: sells.Count,
                        DividendCount: dividends.Count,
                        InterestCount: interests.Count,
                        BuyCosts: buyCosts,
                        SellCosts: sellCosts,
                        DividendWithholding: dividendCosts,
                        InterestWithholding: interestCosts,
                        BuyGross: buyGross,
                        SellGross: sellGross,
                        DividendGross: dividendGross,
                        InterestGross: interestGross
                    );
                })
                .OrderByDescending(s => s.TotalCosts);
        }

        public IEnumerable<TransactionCostSummary> GetTransactionCostSummaries(Portfolio portfolio, DateOnly from, DateOnly to)
        {
            var all = new Dictionary<string, TransactionCostSummary>();

            foreach (var acct in portfolio.Accounts)
            {
                foreach (var s in GetTransactionCostSummaries(acct, from, to))
                {
                    var key = s.Currency.Code;
                    if (!all.TryGetValue(key, out var prev))
                        all[key] = s;
                    else
                        all[key] = new TransactionCostSummary(
                            s.Currency,
                            TotalCosts: prev.TotalCosts + s.TotalCosts,
                            BuyCount: prev.BuyCount + s.BuyCount,
                            SellCount: prev.SellCount + s.SellCount,
                            DividendCount: prev.DividendCount + s.DividendCount,
                            InterestCount: prev.InterestCount + s.InterestCount,
                            BuyCosts: prev.BuyCosts + s.BuyCosts,
                            SellCosts: prev.SellCosts + s.SellCosts,
                            DividendWithholding: prev.DividendWithholding + s.DividendWithholding,
                            InterestWithholding: prev.InterestWithholding + s.InterestWithholding,
                            BuyGross: prev.BuyGross + s.BuyGross,
                            SellGross: prev.SellGross + s.SellGross,
                            DividendGross: prev.DividendGross + s.DividendGross,
                            InterestGross: prev.InterestGross + s.InterestGross
                        );
                }
            }

            return all.Values.OrderByDescending(v => v.TotalCosts);
        }

        public IEnumerable<(string Symbol, Currency Currency, decimal TotalCosts, decimal Gross, TransactionType Type)>
            GetTransactionCostsBySecurity(Account account, DateOnly from, DateOnly to)
        {
            var tx = account.Transactions
                .Where(t => t.Date >= from && t.Date <= to)
                .Where(t => t.Type == TransactionType.Buy ||
                            t.Type == TransactionType.Sell ||
                            t.Type.IsIncome())
                .Where(t => t.Costs is not null && t.Costs.Amount > 0m);

            return tx
                .GroupBy(t => new { t.Symbol.Code, t.Costs!.Currency, t.Type })
                .Select(g => (
                    Symbol: g.Key.Code,
                    Currency: g.Key.Currency,
                    TotalCosts: g.Sum(t => t.Costs!.Amount),
                    Gross: g.Sum(t => t.Amount.Amount),
                    Type: g.Key.Type
                ))
                .OrderByDescending(x => x.TotalCosts);
        }

        public IEnumerable<(string Symbol, Currency Currency, decimal TotalCosts, decimal Gross, TransactionType Type)>
            GetTransactionCostsBySecurity(Portfolio portfolio, DateOnly from, DateOnly to)
        {
            return portfolio.Accounts
                .SelectMany(acct => GetTransactionCostsBySecurity(acct, from, to))
                .GroupBy(x => new { x.Symbol, x.Currency, x.Type })
                .Select(g => (
                    Symbol: g.Key.Symbol,
                    Currency: g.Key.Currency,
                    TotalCosts: g.Sum(v => v.TotalCosts),
                    Gross: g.Sum(v => v.Gross),
                    Type: g.Key.Type
                ))
                .OrderByDescending(x => x.TotalCosts);
        }

        // ==================== PRETTY PRINTERS ====================

        public void PrintTransactionCostReport(Account account, DateOnly from, DateOnly to)
        {
            var summaries = GetTransactionCostSummaries(account, from, to).ToList();
            Console.WriteLine($"\nTransaction Costs — Account: {account.Name}  [{from:yyyy-MM-dd} .. {to:yyyy-MM-dd}]");
            if (!summaries.Any())
            {
                Console.WriteLine("  (no transaction costs in range)");
                return;
            }

            foreach (var s in summaries)
            {
                Console.WriteLine($"  Currency: {s.Currency.Code}");
                Console.WriteLine($"    Total Costs: {s.TotalCosts:0.##} {s.Currency}");
                Console.WriteLine($"    Buys:    {s.BuyCount,3}  Cost={s.BuyCosts:0.##}  Gross={s.BuyGross:0.##}");
                Console.WriteLine($"    Sells:   {s.SellCount,3}  Cost={s.SellCosts:0.##}  Gross={s.SellGross:0.##}");
                Console.WriteLine($"    Dividends: {s.DividendCount,3}  Withholding={s.DividendWithholding:0.##}  Gross={s.DividendGross:0.##}");
                Console.WriteLine($"    Interest:  {s.InterestCount,3}  Withholding={s.InterestWithholding:0.##}  Gross={s.InterestGross:0.##}");
            }

            var top = GetTransactionCostsBySecurity(account, from, to).Take(10).ToList();
            if (top.Any())
            {
                Console.WriteLine("  Top by security:");
                foreach (var row in top)
                {
                    var rate = row.Gross == 0m ? 0m : row.TotalCosts / row.Gross;
                    var type = row.Type.ToString();
                    Console.WriteLine($"    {row.Symbol,-10}  {type,-9}  Cost={row.TotalCosts:0.##} {row.Currency}  Gross={row.Gross:0.##}  Rate={rate:P3}");
                }
            }
        }

        public void PrintTransactionCostReport(Portfolio portfolio, DateOnly from, DateOnly to)
        {
            var summaries = GetTransactionCostSummaries(portfolio, from, to).ToList();
            Console.WriteLine($"\nTransaction Costs — Portfolio: {portfolio.Owner}  [{from:yyyy-MM-dd} .. {to:yyyy-MM-dd}]");
            if (!summaries.Any())
            {
                Console.WriteLine("  (no transaction costs in range)");
                return;
            }

            foreach (var s in summaries)
            {
                Console.WriteLine($"  Currency: {s.Currency.Code}");
                Console.WriteLine($"    Total Costs: {s.TotalCosts:0.##} {s.Currency}");
                Console.WriteLine($"    Buys:    {s.BuyCount,3}  Cost={s.BuyCosts:0.##}  Gross={s.BuyGross:0.##}");
                Console.WriteLine($"    Sells:   {s.SellCount,3}  Cost={s.SellCosts:0.##}  Gross={s.SellGross:0.##}");
                Console.WriteLine($"    Dividends: {s.DividendCount,3}  Withholding={s.DividendWithholding:0.##}  Gross={s.DividendGross:0.##}");
                Console.WriteLine($"    Interest:  {s.InterestCount,3}  Withholding={s.InterestWithholding:0.##}  Gross={s.InterestGross:0.##}");
            }

            var top = GetTransactionCostsBySecurity(portfolio, from, to).Take(12).ToList();
            if (top.Any())
            {
                Console.WriteLine("  Top by security (portfolio):");
                foreach (var row in top)
                {
                    var rate = row.Gross == 0m ? 0m : row.TotalCosts / row.Gross;
                    var type = row.Type.ToString();
                    Console.WriteLine($"    {row.Symbol,-10}  {type,-9}  Cost={row.TotalCosts:0.##} {row.Currency}  Gross={row.Gross:0.##}  Rate={rate:P3}");
                }
            }
        }
    }
}
