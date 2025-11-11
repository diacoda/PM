namespace PM.Domain.Values
{
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
        int InterestCount,
        decimal BuyCosts,
        decimal SellCosts,
        decimal DividendWithholding,
        decimal InterestWithholding,
        decimal BuyGross,        // sum of buy gross amounts
        decimal SellGross,       // sum of sell gross amounts
        decimal DividendGross,   // sum of gross dividends
        decimal InterestGross    // sum of gross interest
    )
    {
        public decimal BuyCostRate => BuyGross == 0m ? 0m : BuyCosts / BuyGross;
        public decimal SellCostRate => SellGross == 0m ? 0m : SellCosts / SellGross;
        public decimal DividendWithholdRate => DividendGross == 0m ? 0m : DividendWithholding / DividendGross;
        public decimal InterestWithholdRate => InterestGross == 0m ? 0m : InterestWithholding / InterestGross;
    }
}
