using PM.SharedKernel;

namespace PM.Domain.Values;

public class Money : IEquatable<Money>
{
    public decimal Amount { get; private set; }
    public Currency Currency { get; private set; } = default!;

    private Money() { }
    public Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public bool Equals(Money? other) =>
        other != null && Amount == other.Amount && Currency == other.Currency;

    public override bool Equals(object? obj) => obj is Money other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public override string ToString() => $"{Amount} {Currency}";
}
