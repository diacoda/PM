namespace PM.DTO;

/// <summary>
/// Data Transfer Object representing a cash flow transaction for an account.
/// </summary>
/// <param name="Amount">The amount of the transaction.</param>
/// <param name="Currency">The currency code of the transaction (e.g., "CAD", "USD").</param>
/// <param name="Date">The date of the transaction.</param>
/// <param name="Note">Optional note or description for the transaction.</param>
public record CashFlowDTO(
    decimal Amount,
    string Currency,
    DateTime? Date,
    string Note
);
