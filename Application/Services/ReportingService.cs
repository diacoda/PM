using System.Threading.Tasks;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Services;

public class ReportingService : IReportingService
{
    private readonly IPricingService _pricingService;

    public ReportingService(IPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    // 1. Aggregation by Asset Class
    public async Task<Dictionary<AssetClass, Money>> AggregateByAssetClassAsync(Account account, DateTime date, Currency reportingCurrency)
    {
        var result = new Dictionary<AssetClass, Money>();

        foreach (var holding in account.Holdings)
        {
            var value = await _pricingService.CalculateHoldingValueAsync(holding, date, reportingCurrency);
            var assetClass = holding.Symbol.AssetClass;

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

    public async Task<Dictionary<AssetClass, Money>> AggregateByAssetClassAsync(Portfolio portfolio, DateTime date, Currency reportingCurrency)
    {
        var result = new Dictionary<AssetClass, Money>();

        foreach (var account in portfolio.Accounts)
        {
            var accountAggregation = await AggregateByAssetClassAsync(account, date, reportingCurrency);

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

    // 2. Holdings Summary per Account
    public void PrintHoldingsSummary(Account account)
    {
        Console.WriteLine($"Holdings Summary for Account: {account.Name} ({account.Currency})");

        foreach (var holding in account.Holdings)
        {
            Console.WriteLine($"- {holding.Symbol} | Asset Class: {holding.Symbol.AssetClass} | Quantity: {holding.Quantity}");
        }
    }

    // 3. Transaction History by Date Range
    public void PrintTransactionHistory(Account account, DateTime from, DateTime to)
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
    public async Task<Dictionary<AssetClass, decimal>> GetAssetClassPercentagesAsync(Account account, DateTime date, Currency reportingCurrency)
    {
        var totals = await AggregateByAssetClassAsync(account, date, reportingCurrency);
        var grand = totals.Values.Sum(m => m.Amount);
        if (grand <= 0m) return new();

        return totals.ToDictionary(k => k.Key, v => v.Value.Amount / grand);
    }

    public async Task<Dictionary<AssetClass, decimal>> GetAssetClassPercentagesAsync(Portfolio portfolio, DateTime date, Currency reportingCurrency)
    {
        var totals = await AggregateByAssetClassAsync(portfolio, date, reportingCurrency);
        var grand = totals.Values.Sum(m => m.Amount);
        if (grand <= 0m) return new();

        return totals.ToDictionary(k => k.Key, v => v.Value.Amount / grand);
    }
    public Dictionary<Currency, decimal> GetTradingCostsByCurrency(Account account, DateTime from, DateTime to)
    {
        var q = account.Transactions
            .Where(t => t.Date >= from && t.Date <= to)
            .Where(t => t.Type is TransactionType.Buy or TransactionType.Sell or TransactionType.Dividend)
            .Where(t => t.Costs is not null && t.Costs!.Amount > 0m);

        return q.GroupBy(t => t.Costs!.Currency)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Costs!.Amount));
    }

    public Dictionary<Currency, decimal> GetTradingCostsByCurrency(Portfolio portfolio, DateTime from, DateTime to)
    {
        var agg = new Dictionary<Currency, decimal>();
        foreach (var acct in portfolio.Accounts)
        {
            foreach (var kv in GetTradingCostsByCurrency(acct, from, to))
                agg[kv.Key] = agg.TryGetValue(kv.Key, out var v) ? v + kv.Value : kv.Value;
        }
        return agg;
    }
    // ==================== TRANSACTION COST REPORTING ====================

    // ---- Core: compute summaries by currency for an Account ----
    public IEnumerable<TransactionCostSummary> GetTransactionCostSummaries(Account account, DateTime from, DateTime to)
    {
        var tx = account.Transactions
            .Where(t => t.Date >= from && t.Date <= to)
            .Where(t => t.Type is TransactionType.Buy or TransactionType.Sell or TransactionType.Dividend)
            .Where(t => t.Costs is not null && t.Costs!.Amount > 0m)
            .ToList();

        return tx
            .GroupBy(t => t.Costs!.Currency)
            .Select(g =>
            {
                var ccy = g.Key;
                var buys = g.Where(t => t.Type == TransactionType.Buy).ToList();
                var sells = g.Where(t => t.Type == TransactionType.Sell).ToList();
                var dividends = g.Where(t => t.Type == TransactionType.Dividend).ToList();

                decimal buyCosts = buys.Sum(t => t.Costs!.Amount);
                decimal sellCosts = sells.Sum(t => t.Costs!.Amount);
                decimal divCosts = dividends.Sum(t => t.Costs!.Amount);

                decimal buyGross = buys.Sum(t => t.Amount.Amount);
                decimal sellGross = sells.Sum(t => t.Amount.Amount);
                decimal divGross = dividends.Sum(t => t.Amount.Amount);

                var total = buyCosts + sellCosts + divCosts;

                return new TransactionCostSummary(
                    Currency: ccy,
                    TotalCosts: total,
                    BuyCount: buys.Count,
                    SellCount: sells.Count,
                    DividendCount: dividends.Count,
                    BuyCosts: buyCosts,
                    SellCosts: sellCosts,
                    DividendWithholding: divCosts,
                    BuyGross: buyGross,
                    SellGross: sellGross,
                    DividendGross: divGross
                );
            })
            .OrderByDescending(s => s.TotalCosts);
    }

    // ---- Portfolio aggregator: fold account summaries by currency ----
    public IEnumerable<TransactionCostSummary> GetTransactionCostSummaries(Portfolio portfolio, DateTime from, DateTime to)
    {
        var all = new Dictionary<string, TransactionCostSummary>(); // key = currency code

        foreach (var acct in portfolio.Accounts)
        {
            foreach (var s in GetTransactionCostSummaries(acct, from, to))
            {
                var key = s.Currency.Code;
                if (!all.TryGetValue(key, out var prev))
                {
                    all[key] = s;
                }
                else
                {
                    all[key] = new TransactionCostSummary(
                        s.Currency,
                        TotalCosts: prev.TotalCosts + s.TotalCosts,
                        BuyCount: prev.BuyCount + s.BuyCount,
                        SellCount: prev.SellCount + s.SellCount,
                        DividendCount: prev.DividendCount + s.DividendCount,
                        BuyCosts: prev.BuyCosts + s.BuyCosts,
                        SellCosts: prev.SellCosts + s.SellCosts,
                        DividendWithholding: prev.DividendWithholding + s.DividendWithholding,
                        BuyGross: prev.BuyGross + s.BuyGross,
                        SellGross: prev.SellGross + s.SellGross,
                        DividendGross: prev.DividendGross + s.DividendGross
                    );
                }
            }
        }

        return all.Values.OrderByDescending(v => v.TotalCosts);
    }

    // ---- Optional: breakdown by Security (symbol) for an Account ----
    public IEnumerable<(string Symbol, Currency Currency, decimal TotalCosts, decimal Gross, TransactionType Type)>
        GetTransactionCostsBySecurity(Account account, DateTime from, DateTime to)
    {
        var tx = account.Transactions
            .Where(t => t.Date >= from && t.Date <= to)
            .Where(t => t.Type is TransactionType.Buy or TransactionType.Sell or TransactionType.Dividend)
            .Where(t => t.Costs is not null && t.Costs!.Amount > 0m);

        return tx
            // group by symbol + cost currency + type
            .GroupBy(t => new { Sym = t.Symbol.Value, Cur = t.Costs!.Currency, t.Type })
            // NAME the tuple fields here ⬇
            .Select(g => (
                Symbol: g.Key.Sym,
                Currency: g.Key.Cur,
                TotalCosts: g.Sum(t => t.Costs!.Amount),
                Gross: g.Sum(t => t.Amount.Amount),
                Type: g.Key.Type
            ))
            .OrderByDescending(x => x.TotalCosts);
    }

    // ---- Optional: breakdown by Security for Portfolio (aggregated) ----
    public IEnumerable<(string Symbol, Currency Currency, decimal TotalCosts, decimal Gross, TransactionType Type)>
        GetTransactionCostsBySecurity(Portfolio portfolio, DateTime from, DateTime to)
    {
        // Reuse the account method, then aggregate across accounts
        return portfolio.Accounts
            .SelectMany(acct => GetTransactionCostsBySecurity(acct, from, to))
            .GroupBy(x => new { x.Symbol, Cur = x.Currency, x.Type })
            .Select(g => (
                Symbol: g.Key.Symbol,
                Currency: g.Key.Cur,
                TotalCosts: g.Sum(v => v.TotalCosts),
                Gross: g.Sum(v => v.Gross),
                Type: g.Key.Type
            ))
            .OrderByDescending(x => x.TotalCosts);
    }

    // ---- Pretty printers (Account & Portfolio) ----
    public void PrintTransactionCostReport(Account account, DateTime from, DateTime to)
    {
        var summaries = GetTransactionCostSummaries(account, from, to).ToList();
        Console.WriteLine($"\nTransaction Costs — Account: {account.Name}  [{from:yyyy-MM-dd} .. {to:yyyy-MM-dd}]");
        if (summaries.Count == 0)
        {
            Console.WriteLine("  (no transaction costs in range)");
            return;
        }

        foreach (var s in summaries)
        {
            Console.WriteLine($"  Currency: {s.Currency.Code}");
            Console.WriteLine($"    Total Costs: {s.TotalCosts:0.##} {s.Currency}");
            Console.WriteLine($"    Buys:    {s.BuyCount,3}  Cost={s.BuyCosts:0.##}  Gross={s.BuyGross:0.##}  CostRate={s.BuyCostRate:P3}");
            Console.WriteLine($"    Sells:   {s.SellCount,3}  Cost={s.SellCosts:0.##}  Gross={s.SellGross:0.##}  CostRate={s.SellCostRate:P3}");
            Console.WriteLine($"    Dividends: {s.DividendCount,3}  Withholding={s.DividendWithholding:0.##}  Gross={s.DividendGross:0.##}  Rate={s.DividendWithholdRate:P3}");
        }

        // Top securities by cost
        var top = GetTransactionCostsBySecurity(account, from, to).Take(10).ToList();
        if (top.Count > 0)
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

    public void PrintTransactionCostReport(Portfolio portfolio, DateTime from, DateTime to)
    {
        var summaries = GetTransactionCostSummaries(portfolio, from, to).ToList();
        Console.WriteLine($"\nTransaction Costs — Portfolio: {portfolio.Owner}  [{from:yyyy-MM-dd} .. {to:yyyy-MM-dd}]");
        if (summaries.Count == 0)
        {
            Console.WriteLine("  (no transaction costs in range)");
            return;
        }

        foreach (var s in summaries)
        {
            Console.WriteLine($"  Currency: {s.Currency.Code}");
            Console.WriteLine($"    Total Costs: {s.TotalCosts:0.##} {s.Currency}");
            Console.WriteLine($"    Buys:    {s.BuyCount,3}  Cost={s.BuyCosts:0.##}  Gross={s.BuyGross:0.##}  CostRate={s.BuyCostRate:P3}");
            Console.WriteLine($"    Sells:   {s.SellCount,3}  Cost={s.SellCosts:0.##}  Gross={s.SellGross:0.##}  CostRate={s.SellCostRate:P3}");
            Console.WriteLine($"    Dividends: {s.DividendCount,3}  Withholding={s.DividendWithholding:0.##}  Gross={s.DividendGross:0.##}  Rate={s.DividendWithholdRate:P3}");
        }

        var top = GetTransactionCostsBySecurity(portfolio, from, to).Take(12).ToList();
        if (top.Count > 0)
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