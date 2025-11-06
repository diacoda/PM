using PM.Domain.Interfaces;
using PM.Domain.Values;

namespace PM.Domain.Entities;

public class Asset : IAsset
{
    public string Code { get; set; } = string.Empty;
    public Currency Currency { get; set; } = Currency.CAD;
    public AssetClass AssetClass { get; set; }


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