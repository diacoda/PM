using PM.Domain.Values;

namespace PM.Application.Interfaces;

/// <summary>
/// Minimal trade-cost calculator:
///  - For BUY/SELL: cost = fixed + pct * gross, clamped by optional minimum.
///  - For DIVIDEND: cost = withholdingPct * gross (e.g., 0.15 for 15%).
/// Rules are provided per currency code (e.g., "CAD", "USD") to keep it simple.
/// </summary>
public interface ITradeCostService
{
    record BuySellRule(decimal Fixed, decimal Pct, decimal? Min = null);

    Money ComputeBuySellCost(Money gross);
    Money ComputeDividendWithholding(Money grossDividend);
}
