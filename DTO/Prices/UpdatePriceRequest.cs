namespace PM.DTO.Prices;

/// <summary>
/// Represents a request to update the closing price of a symbol for a specific date.
/// </summary>
public class UpdatePriceRequest
{
    /// <summary>
    /// The date of the price to update.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// The new closing price for the symbol on the specified date.
    /// </summary>
    public decimal Close { get; set; }
}
