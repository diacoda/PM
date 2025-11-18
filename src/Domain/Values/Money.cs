using PM.SharedKernel;

namespace PM.Domain.Values;

public sealed record Money
{
    public decimal Amount { get; init; }
    public Currency Currency { get; init; }

    public Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }
}