namespace PM.API.Configuration
{
    /// <summary>
    /// Configuration settings for a financial instrument symbol.
    /// Used to specify the symbol value, its currency, and exchange.
    /// </summary>
    public class SymbolConfig
    {
        /// <summary>
        /// The ticker or symbol identifier (e.g., "VFV.TO").
        /// </summary>
        public string Value { get; set; } = null!;

        /// <summary>
        /// The currency of the instrument (default is "CAD").
        /// </summary>
        public string Currency { get; set; } = "CAD";

        /// <summary>
        /// The exchange where the instrument is traded (default is "TSX").
        /// </summary>
        public string Exchange { get; set; } = "TSX";
    }
}
