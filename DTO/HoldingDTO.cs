namespace PM.DTO;

/// <summary>
/// Data Transfer Object representing a holding in an investment account.
/// </summary>
public class HoldingDTO
{
    /// <summary>
    /// Unique identifier of the holding.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Ticker symbol of the security (e.g., "AAPL").
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    public string SymbolCurrency { get; set; } = string.Empty;
    public string SymbolAssetClass { get; set; } = string.Empty;


    /// <summary>
    /// Full name of the instrument/security.
    /// </summary>
    public string InstrumentName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of the holding in the account.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Identifier of the account this holding belongs to.
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// Optional tags associated with the holding (e.g., "tech", "dividend").
    /// </summary>
    public List<string> Tags { get; set; } = new();
}
