using model.Domain.Values;

namespace model.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public Instrument Instrument { get; set; } = default!;
    public decimal Quantity { get; set; }
    /// <summary>
    /// Gross monetary amount in the transaction currency.
    /// BUY: cash outflow equals Amount + Costs (if any)
    /// SELL: cash inflow equals Amount - Costs (if any)
    /// DIVIDEND: gross dividend; net cash = Amount - Costs (e.g., withholding)
    /// </summary>
    public Money Amount { get; set; } = new Money(0.0m, new Currency("CAD"));

    /// <summary>
    /// Optional transaction costs in the same currency as Amount (commission, ECN, fees, withholding).
    /// Null or Amount=0 means no costs.
    /// </summary>
    public Money? Costs { get; set; }
}