namespace PM.DTO;

/// <summary>
/// Data Transfer Object representing a transaction in an account.
/// </summary>
public class TransactionDTO
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string Type { get; set; } = default!;

    public string Symbol { get; set; } = default!; // flatten Symbol.Value

    public decimal Quantity { get; set; }

    // Flatten Money: amount + currency
    public decimal Amount { get; set; }
    public string AmountCurrency { get; set; } = default!;

    // Optional costs
    public decimal? Costs { get; set; }
    public string? CostsCurrency { get; set; }

    public int AccountId { get; set; }
}

