namespace PM.DTO;

/// <summary>
/// Data Transfer Object representing a portfolio with its accounts.
/// </summary>
public class PortfolioDTO
{
    /// <summary>
    /// Unique identifier of the portfolio.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Owner of the portfolio.
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// List of accounts contained in the portfolio.
    /// </summary>
    public List<CreateAccountDTO> Accounts { get; set; } = new();
}
