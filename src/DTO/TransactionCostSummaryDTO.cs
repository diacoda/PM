namespace PM.DTO;

public record TransactionCostSummaryDTO(
    string Currency,
    decimal TotalCosts,
    int BuyCount,
    int SellCount,
    int DividendCount,
    int InterestCount,
    decimal BuyCosts,
    decimal SellCosts,
    decimal DividendWithholding,
    decimal InterestWithholding,
    decimal BuyGross,
    decimal SellGross,
    decimal DividendGross,
    decimal InterestGross);
