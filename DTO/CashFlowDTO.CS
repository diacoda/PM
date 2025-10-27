namespace PM.DTO;

public record CashFlowDTO(int PortfolioId, int AccountId, decimal Amount, string Currency, DateTime Date, string Note);
