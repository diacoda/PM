namespace PM.DTO;

public record TransactionCostsBySecurityDTO(
    string Symbol,
    string Currency,
    decimal TotalCosts,
    decimal Gross,
    string Type);