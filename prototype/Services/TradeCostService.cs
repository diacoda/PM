using model.Domain.Values;

namespace model.Services;

/// <summary>
/// Minimal trade-cost calculator:
///  - For BUY/SELL: cost = fixed + pct * gross, clamped by optional minimum.
///  - For DIVIDEND: cost = withholdingPct * gross (e.g., 0.15 for 15%).
/// Rules are provided per currency code (e.g., "CAD", "USD") to keep it simple.
/// </summary>
public class TradeCostService
{
    public record BuySellRule(decimal Fixed, decimal Pct, decimal? Min = null);
    private readonly Dictionary<string, BuySellRule> _buySell;
    private readonly Dictionary<string, decimal> _dividendWithholdingPct; // e.g., "USD" => 0.15m

    public TradeCostService(
        Dictionary<string, BuySellRule>? buySellRules = null,
        Dictionary<string, decimal>? dividendWithholdingPct = null)
    {
        _buySell = buySellRules ?? new();
        _dividendWithholdingPct = dividendWithholdingPct ?? new();
    }

    public Money ComputeBuySellCost(Money gross)
    {
        var ccy = gross.Currency.Code;
        if (!_buySell.TryGetValue(ccy, out var r)) return new Money(0m, gross.Currency);

        var raw = r.Fixed + r.Pct * gross.Amount;
        if (r.Min.HasValue && raw < r.Min.Value) raw = r.Min.Value;
        return new Money(decimal.Round(raw, 2), gross.Currency);
    }

    public Money ComputeDividendWithholding(Money grossDividend)
    {
        var ccy = grossDividend.Currency.Code;
        if (!_dividendWithholdingPct.TryGetValue(ccy, out var pct)) return new Money(0m, grossDividend.Currency);
        var w = grossDividend.Amount * pct;
        return new Money(decimal.Round(w, 2), grossDividend.Currency);
    }
}
