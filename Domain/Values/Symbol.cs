namespace PM.Domain.Values;

public sealed class Symbol : IEquatable<Symbol>
{
    public string Value { get; }
    public string Currency { get; }
    public string Exchange { get; }
    public AssetClass AssetClass { get; }   // 👈 NEW
    private Symbol() { } // 👈 EF Core will use this

    public Symbol(string value, string currency = "CAD", string exchange = "TSX")
        : this(value, currency, exchange, ResolveAssetClass(value)) { }

    public Symbol(string value, string currency, string exchange, AssetClass assetClass)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Symbol is required.", nameof(value));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length > 20)
            throw new ArgumentException("Symbol is too long (max 20).", nameof(value));

        Value = normalized;
        Currency = currency.Trim().ToUpperInvariant();
        Exchange = exchange.Trim().ToUpperInvariant();
        AssetClass = assetClass;
    }

    // 🔍 Resolve asset class from static map
    private static AssetClass ResolveAssetClass(string symbol)
    {
        if (SymbolAssetClassMap.TryGetValue(symbol.Trim().ToUpperInvariant(), out var cls))
            return cls;

        return AssetClass.Other;
    }

    private static readonly Dictionary<string, AssetClass> SymbolAssetClassMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "VFV.TO", AssetClass.USEquity },
        { "VCE.TO", AssetClass.CanadianEquity },
        { "HXQ.TO", AssetClass.USEquity },
        { "MFC.TO", AssetClass.CanadianEquity },
        { "VDY.TO", AssetClass.CanadianEquity },
        { "BKCC.TO", AssetClass.CanadianEquity },
        { "ZGLD.TO", AssetClass.Commodity },
        { "VI.TO", AssetClass.DevelopedEquity },
        { "BTCC.TO", AssetClass.Crypto },
        { "CAD", AssetClass.Cash },
        { "TDB900", AssetClass.CanadianEquity },
        { "TDB902", AssetClass.USEquity },
        { "TDB911", AssetClass.DevelopedEquity },
        { "ZGLH.TO", AssetClass.Commodity }
    };

    public override string ToString() => $"{Value} ({Currency})";

    public bool Equals(Symbol? other) =>
        other is not null && Value == other.Value && Currency == other.Currency;

    public override bool Equals(object? obj) => obj is Symbol s && Equals(s);

    public override int GetHashCode() =>
        HashCode.Combine(StringComparer.Ordinal.GetHashCode(Value), StringComparer.Ordinal.GetHashCode(Currency));
}
