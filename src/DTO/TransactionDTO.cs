namespace PM.DTO;

/// <summary>
/// Data Transfer Object representing a transaction in an account.
/// Used for retrieving transaction information via the API.
/// </summary>
public class TransactionDTO
{
    /// <summary>
    /// Unique identifier of the transaction.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The date and time when the transaction occurred.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// The type of the transaction, e.g., "Buy", "Sell", "Deposit", "Withdrawal", or "Dividend".
    /// </summary>
    public string Type { get; set; } = default!;

    /// <summary>
    /// The symbol of the instrument involved in the transaction (flattened from <c>Symbol.Value</c>).
    /// </summary>
    public string Symbol { get; set; } = default!;

    /// <summary>
    /// The quantity of the instrument transacted.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// The monetary amount associated with the transaction (flattened from <c>Money.Amount</c>).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The currency of the transaction amount (flattened from <c>Money.Currency</c>).
    /// </summary>
    public string AmountCurrency { get; set; } = default!;

    /// <summary>
    /// Optional transaction costs, such as fees or withholding amounts.
    /// </summary>
    public decimal? Costs { get; set; }

    /// <summary>
    /// The currency of the transaction costs. May be null if not applicable.
    /// </summary>
    public string? CostsCurrency { get; set; }

    /// <summary>
    /// The identifier of the account to which this transaction belongs.
    /// </summary>
    public int AccountId { get; set; }

    public int CashFlowId { get; set; }

    public int[] HoldingIds { get; set; } = Array.Empty<int>();
}
