namespace PM.Domain.Values;

/// <summary>
/// Represents a financial symbol with associated currency, exchange, and asset class.
/// </summary>
public sealed class Symbol : IEquatable<Symbol>
{
    /// <summary>
    /// The symbol value (e.g., "VFV.TO").
    /// </summary>
    public string Code { get; } = default!;

    /// <summary>
    /// The currency of the symbol (e.g., "CAD").
    /// </summary>
    public string Currency { get; } = default!;

    /// <summary>
    /// The exchange where the symbol trades (e.g., "TSX").
    /// </summary>
    public string Exchange { get; } = default!;

    /// <summary>
    /// The asset class of the symbol.
    /// </summary>
    public AssetClass AssetClass { get; }

    /// <summary>
    /// Private constructor for EF Core or serialization.
    /// </summary>
    private Symbol() { }

    /// <summary>
    /// Creates a new <see cref="Symbol"/> with default exchange and auto-resolved asset class.
    /// </summary>
    /// <param name="code">The symbol value.</param>
    /// <param name="currency">The currency code (default "CAD").</param>
    /// <param name="exchange">The exchange code (default "TSX").</param>
    public Symbol(string code, string currency = "CAD", string exchange = "TSX")
        : this(code, currency, exchange, ResolveAssetClass(code)) { }

    /// <summary>
    /// Creates a new <see cref="Symbol"/> with explicit asset class.
    /// </summary>
    /// <param name="code">The symbol value.</param>
    /// <param name="currency">The currency code.</param>
    /// <param name="exchange">The exchange code.</param>
    /// <param name="assetClass">The asset class of the symbol.</param>
    public Symbol(string code, string currency, string exchange, AssetClass assetClass)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Symbol is required.", nameof(code));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        var normalized = code.Trim().ToUpperInvariant();
        if (normalized.Length > 20)
            throw new ArgumentException("Symbol is too long (max 20).", nameof(code));

        Code = normalized;
        Currency = currency.Trim().ToUpperInvariant();
        Exchange = exchange.Trim().ToUpperInvariant();
        AssetClass = assetClass;
    }

    /// <summary>
    /// Resolves the asset class from a static map of known symbols.
    /// Returns <see cref="AssetClass.Other"/> if unknown.
    /// </summary>
    public static AssetClass ResolveAssetClass(string symbol)
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
        { "USD", AssetClass.Cash },
        { "TDB900", AssetClass.CanadianEquity },
        { "TDB902", AssetClass.USEquity },
        { "TDB911", AssetClass.DevelopedEquity },
        { "ZGLH.TO", AssetClass.Commodity }
    };

    public override string ToString() => $"{Code} ({Currency})";

    public bool Equals(Symbol? other) =>
        other is not null && Code == other.Code && Currency == other.Currency;

    public override bool Equals(object? obj) => obj is Symbol s && Equals(s);

    public override int GetHashCode() =>
        HashCode.Combine(StringComparer.Ordinal.GetHashCode(Code), StringComparer.Ordinal.GetHashCode(Currency));
}
