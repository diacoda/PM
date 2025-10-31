namespace PM.DTO;

/// <summary>
/// DTO used for creating a new transaction via API.

/// </summary>
public class CreateTransactionDTO
{
    public DateTime? Date { get; set; } = DateTime.UtcNow;

    public string Type { get; set; } = default!;

    public string Symbol { get; set; } = default!; // flatten Symbol.Value

    public decimal Quantity { get; set; }

    // Flatten Money: amount + currency
    public decimal Amount { get; set; }
    public string AmountCurrency { get; set; } = default!;

    // Optional costs
    public decimal? Costs { get; set; } = 0;
    public string? CostsCurrency { get; set; } = "CAD";
}