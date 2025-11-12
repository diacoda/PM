using PM.Domain.Values;
using PM.DTO;

namespace PM.Domain.Mappers;

public static class TransactionCostSummaryMapper
{
    public static TransactionCostSummaryDTO FromEntity(TransactionCostSummary s) =>
       new(
       Currency: s.Currency.Code,
       TotalCosts: s.TotalCosts,
       BuyCount: s.BuyCount,
       SellCount: s.SellCount,
       DividendCount: s.DividendCount,
       InterestCount: s.InterestCount,
       BuyCosts: s.BuyCosts,
       SellCosts: s.SellCosts,
       DividendWithholding: s.DividendWithholding,
       InterestWithholding: s.InterestWithholding,
       BuyGross: s.BuyGross,
       SellGross: s.SellGross,
       DividendGross: s.DividendGross,
       InterestGross: s.InterestGross
       );
}