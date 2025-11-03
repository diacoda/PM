using PM.Domain.Values;

namespace PM.Domain.Interfaces;
public interface IAsset
{
    public string Code { get; set; }
    public Currency Currency { get; }
    public AssetClass AssetClass { get; set; }
}