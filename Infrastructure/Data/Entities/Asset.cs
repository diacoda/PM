using PM.Domain.Interfaces;
using PM.Domain.Values;

namespace PM.Infrastructure.Data.Entities;
internal class Asset : IAsset
{
    public string Code { get; set; } = string.Empty;
    public Currency Currency { get; set; } = Currency.CAD;
    public AssetClass AssetClass { get; set; }
}