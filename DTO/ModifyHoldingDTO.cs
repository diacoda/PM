namespace PM.DTO;
/// <summary>
/// Data Transfer Object representing a holding in an investment account.
/// </summary>
public class ModifyHoldingDTO
{
    /// <summary>
    /// Ticker symbol of the security (e.g., "AAPL").
    /// </summary>
    //public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of the holding in the account.
    /// </summary>
    public decimal Quantity { get; set; }
}