namespace PM.DTO.Prices;

/// <summary>
/// Represents a request to fetch a price from a provider and upsert it into the database.
/// </summary>
public class UpsertPriceProviderRequest
{
    /// <summary>
    /// The ticker symbol of the security (e.g., "AAPL").
    /// </summary>
    public string Symbol { get; set; } = default!;

    /// <summary>
    /// The date for which the price should be fetched and upserted.
    /// </summary>
    public DateOnly Date { get; set; }
}
