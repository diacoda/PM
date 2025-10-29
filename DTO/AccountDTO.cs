namespace PM.DTO;

/// <summary>
/// Data Transfer Object representing an investment account.
/// </summary>
public class AccountDTO
{
    /// <summary>
    /// Unique identifier of the account.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the account.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Currency code for the account (e.g., "CAD", "USD").
    /// Defaults to "CAD".
    /// </summary>
    public string Currency { get; set; } = "CAD";

    /// <summary>
    /// Name of the financial institution holding the account.
    /// </summary>
    public string FinancialInstitution { get; set; } = string.Empty;

    /// <summary>
    /// The portfolio ID to which this account belongs.
    /// </summary>
    public int PortfolioId { get; set; }
}
