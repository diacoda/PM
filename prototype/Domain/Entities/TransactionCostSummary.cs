using model.Domain.Values;

namespace model.Domain.Entities;

/// <summary>
/// Compact view of trading costs and simple effectiveness ratios per currency.
/// Rates are decimals (e.g., 0.001 = 0.10%).
/// </summary>
public record TransactionCostSummary(
    Currency Currency,
    decimal TotalCosts,
    int BuyCount,
    int SellCount,
    int DividendCount,
    decimal BuyCosts,
    decimal SellCosts,
    decimal DividendWithholding,
    decimal BuyGross,        // sum of buy gross amounts
    decimal SellGross,       // sum of sell gross amounts
    decimal DividendGross    // sum of gross dividends
)
{
    public decimal BuyCostRate           => BuyGross       == 0m ? 0m : BuyCosts            / BuyGross;
    public decimal SellCostRate          => SellGross      == 0m ? 0m : SellCosts           / SellGross;
    public decimal DividendWithholdRate  => DividendGross  == 0m ? 0m : DividendWithholding / DividendGross;
}
