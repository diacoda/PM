namespace PM.Infrastructure.Configuration
{
    /// <summary>
    /// Represents the configured file system paths for the application's SQLite databases.
    /// </summary>
    /// <remarks>
    /// This record is typically bound from configuration (e.g., <c>appsettings.json</c>) 
    /// and injected where multiple local databases are used — such as portfolio, cash flow, 
    /// and valuation data stores.
    /// </remarks>
    /// <example>
    /// Example configuration section:
    /// <code>
    /// "DatabasePaths": {
    ///   "PortfolioPath": "Data/portfolio.db",
    ///   "CashFlowPath": "Data/cashflow.db",
    ///   "ValuationPath": "Data/valuation.db"
    /// }
    /// </code>
    /// </example>
    public record DatabasePaths
    {
        /// <summary>
        /// Gets or init the file path to the portfolio database.
        /// </summary>
        public string PortfolioPath { get; init; }

        /// <summary>
        /// Gets or init the file path to the cash flow database.
        /// </summary>
        public string CashFlowPath { get; init; }

        /// <summary>
        /// Gets or init the file path to the valuation database.
        /// </summary>
        public string ValuationPath { get; init; }

        /// <summary>
        /// Initializes a new instance of <see cref="DatabasePaths"/>.
        /// </summary>
        public DatabasePaths(string portfolioPath, string cashFlowPath, string valuationPath)
        {
            PortfolioPath = portfolioPath;
            CashFlowPath = cashFlowPath;
            ValuationPath = valuationPath;
        }
    }
}
