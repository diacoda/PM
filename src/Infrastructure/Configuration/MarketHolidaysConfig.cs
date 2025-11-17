namespace PM.Infrastructure.Configuration
{
    /// <summary>
    /// Represents a configuration section that maps market identifiers 
    /// (e.g., "TSX", "NYSE") to lists of holiday dates when the market is closed.
    /// </summary>
    /// <remarks>
    /// This class is typically bound from the <c>MarketHolidays</c> section 
    /// in <c>appsettings.json</c> using configuration binding.
    /// 
    /// Example usage in <c>appsettings.json</c>:
    /// <code>
    /// "MarketHolidays": {
    ///   "TSX": [ "2025-01-01", "2025-04-18", "2025-12-25" ],
    ///   "NYSE": [ "2025-01-01", "2025-07-04", "2025-12-25" ]
    /// }
    /// </code>
    /// </remarks>
    public class MarketHolidaysConfig : Dictionary<string, List<DateOnly>>
    {
    }
}
