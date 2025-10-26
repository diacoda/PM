namespace model.Domain.Values;
public readonly record struct Symbol(string Code)
{
    public override string ToString() => Code;

    public static Symbol From(string code) => new Symbol(code.ToUpperInvariant());
}