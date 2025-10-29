namespace PM.DTO.Prices;

/// <summary>
/// Represents the closing price of a specific symbol on a given date.
/// </summary>
public class PriceDTO
{
    /// <summary>
    /// The ticker symbol of the security (e.g., "AAPL").
    /// </summary>
    public string Symbol { get; set; } = default!;

    /// <summary>
    /// The date of the price.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// The closing price of the symbol on the specified date.
    /// </summary>
    public decimal Close { get; set; }
}
