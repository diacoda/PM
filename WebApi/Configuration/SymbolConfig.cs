namespace PM.API.Configuration;

public class SymbolConfig
{
    public string Value { get; set; } = null!;
    public string Currency { get; set; } = "CAD"; // default
    public string Exchange { get; set; } = "TSX"; // default
}