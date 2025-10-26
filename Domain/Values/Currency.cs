namespace Domain.Values;

public readonly record struct Currency(string Code)
{
    public override string ToString() => Code;

    public static Currency CAD => new("CAD");
    public static Currency USD => new("USD");
    public static Currency EUR => new("EUR");

    public static Currency From(string code) => new(code.ToUpperInvariant());
}