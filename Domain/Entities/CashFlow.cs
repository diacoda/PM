using PM.Domain.Enums;
using PM.Domain.Values;
using PM.SharedKernel;

namespace PM.Domain.Entities;

public class CashFlow : Entity
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public DateTime Date { get; set; }
    public Money Amount { get; set; } = new Money(0.0m, Currency.CAD);
    public CashFlowType Type { get; set; }
    public string? Note { get; set; }
}