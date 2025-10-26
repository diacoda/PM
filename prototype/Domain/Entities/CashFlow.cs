using model.Domain.Values;

namespace model.Domain.Entities;

public class CashFlow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int AccountId { get; set; }
    public DateTime Date { get; set; }
    public Money Amount { get; set; } = new Money(0.0m, Currency.CAD);
    public CashFlowType Type { get; set; }
    public string? Note { get; set; }
}