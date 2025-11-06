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
        if (obj is Asset other)
        {
            return Code.Equals(other.Code, StringComparison.OrdinalIgnoreCase)
                    && Currency.Code.Equals(other.Currency.Code, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public override int GetHashCode() =>
        HashCode.Combine(Code.ToUpperInvariant(), Currency.Code.ToUpperInvariant());

}