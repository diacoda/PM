namespace PM.Domain.Values;

public sealed record Valuation(
    Money TotalValue,
    Money SecuritiesValue,
    Money CashValue,
    Money IncomeForDay,
    Currency ReportingCurrency,
    AssetClass AssetClass,
    decimal Percentage
);
