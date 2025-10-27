namespace PM.Domain.Values;

public class Instrument
{
    public Symbol Symbol { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public AssetClass AssetClass { get; private set; }

    private Instrument() { }

    public Instrument(Symbol symbol, string name, AssetClass assetClass)
    {
        Symbol = symbol;
        Name = name;
        AssetClass = assetClass;
    }
}