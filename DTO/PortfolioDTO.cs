namespace PM.DTO;

public class PortfolioDTO
{
    public int Id { get; set; }
    public string Owner { get; set; } = string.Empty;
    public List<CreateAccountDTO> Accounts = new();
}
