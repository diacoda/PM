namespace PM.Domain.Values;

public class InstrumentREMOVED
{
    public Symbol Symbol { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public AssetClass AssetClass { get; private set; }

    private InstrumentREMOVED() { }

    public InstrumentREMOVED(Symbol symbol, string name, AssetClass assetClass)
    {
        Symbol = symbol;
        Name = name;
        AssetClass = assetClass;
    }
}