namespace PM.DTO.Prices;

/// <summary>
/// DTO version of SymbolFetchDetail for API responses.
/// </summary>
public class SymbolFetchDetailDTO
{
    public string Symbol { get; init; } = string.Empty;
    public string Exchange { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Error { get; init; }
}