namespace PM.DTO;

/// <summary>
/// Data Transfer Object used for creating a new transaction via the API.
/// Contains the necessary fields to record trades, deposits, withdrawals, or dividends.
/// </summary>
public class CreateTransactionDTO
{
    /// <summary>
    /// The date and time of the transaction. Defaults to UTC now if not specified.
    /// </summary>
    public DateTime? Date { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The type of transaction, e.g., "Buy", "Sell", "Deposit", "Withdrawal", or "Dividend".
    /// </summary>
    public string Type { get; set; } = default!;

    /// <summary>
    /// The symbol of the instrument involved in the transaction (flattened from <c>Symbol.Value</c>).
    /// </summary>
    public string Symbol { get; set; } = default!;

    /// <summary>
    /// The quantity of the instrument being transacted.
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
    public decimal? Costs { get; set; } = 0;

    /// <summary>
    /// The currency of the transaction costs. Defaults to "CAD" if not specified.
    /// </summary>
    public string? CostsCurrency { get; set; } = "CAD";
}
