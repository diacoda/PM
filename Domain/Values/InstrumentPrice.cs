namespace PM.Domain.Values;

public record InstrumentPrice(
    Symbol Symbol,
    DateOnly Date,
    Money Price,
    Currency Currency,
    string Source)
{
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
