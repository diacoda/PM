using System.ComponentModel.DataAnnotations.Schema;
using PM.Domain.Entities;
using PM.Domain.Interfaces;

namespace PM.Domain.Values;

public class Symbol : IAsset
{
    public string Code { get; set; } = string.Empty;

    public AssetClass AssetClass { get; } = default!;

    [NotMapped]
    public string Exchange { get; } = default!;

    // EF-friendly backing property for Currency
    public string CurrencyCode
    {
        get => Currency.Code;
        private set => Currency = new Currency(value);
    }

    [NotMapped]
    public Currency Currency { get; private set; } = Currency.CAD;

    // EF Core requires parameterless constructor
    private Symbol() { }

    public Symbol(string code, string currency)
    {
        Code = code;
        Currency = new Currency(currency);
        AssetClass = ResolveAssetClass(code);
        Exchange = ResolveExchange(code);
    }
    public Symbol(string code) : this(code, "CAD")
    {
    }

    public Asset ToAsset() =>
        new Asset
        {
            Code = Code,
            Currency = Currency,
            AssetClass = AssetClass
        };

    private static AssetClass ResolveAssetClass(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return AssetClass.Other;

        return _assetClasses.TryGetValue(code, out var ac) ? ac : AssetClass.Other;
    }
    private static readonly Dictionary<string, AssetClass> _assetClasses = new()
    {
        { "MFCSP500", AssetClass.USEquity },
        { "VFV.TO", AssetClass.USEquity },
        { "HXQ.TO", AssetClass.USEquity },
        { "VCE.TO", AssetClass.CanadianEquity },
        { "MFC.TO", AssetClass.CanadianEquity },
        { "VDY.TO", AssetClass.CanadianEquity },
        { "BKCC.TO", AssetClass.CanadianEquity },
        { "ZGLD.TO", AssetClass.Commodity },
        { "VI.TO", AssetClass.CanadianEquity },
        { "BTCC.TO", AssetClass.Crypto },
        { "TDB900", AssetClass.CanadianEquity },
        { "TDB902", AssetClass.USEquity },
        { "TDB911", AssetClass.DevelopedEquity },
        { "ZGLH.TO", AssetClass.Commodity },
        { "CAD", AssetClass.Cash},
        { "USD", AssetClass.Cash},
        { "TRI", AssetClass.USEquity},
        { "PREF.TO", AssetClass.CanadianEquity}
    };

    private static string ResolveExchange(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ApplicationException("Symbol code can't be empty or null");

        if (!_exchanges.TryGetValue(code, out var ac))
        {
            throw new ApplicationException("Symbol code must have an exchange");
        }
        return ac;
    }

    private static readonly Dictionary<string, string> _exchanges = new()
    {
        { "MFCSP500", "TSX" },
        { "VFV.TO", "TSX" },
        { "HXQ.TO", "TSX" },
        { "VCE.TO", "TSX" },
        { "MFC.TO", "TSX" },
        { "VDY.TO", "TSX" },
        { "BKCC.TO", "TSX" },
        { "ZGLD.TO", "TSX" },
        { "VI.TO", "TSX" },
        { "BTCC.TO", "TSX" },
        { "TDB900", "TSX" },
        { "TDB902", "TSX" },
        { "TDB911", "TSX" },
        { "ZGLH.TO", "TSX" },
        { "CAD", "TSX"},
        { "USD", "TSX"},
        { "TRI", "NYSE"},
        { "PREF.TO", "TSX"}
    };


    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;
        if (obj is null)
            return false;

        if (obj is not IAsset other)
            return false;

        return string.Equals(Code, other.Code, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Currency?.Code, other.Currency?.Code, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode() =>
                HashCode.Combine(Code.ToUpperInvariant(), Currency?.Code?.ToUpperInvariant());
}
