namespace PM.DTO;

public class AccountDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = "CAD";
    public string FinancialInstitution { get; set; } = string.Empty;
    public int PortfolioId { get; set; }
}
