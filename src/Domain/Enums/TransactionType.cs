namespace PM.Domain.Enums;

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Buy,
    Sell,
    Dividend,
    Other = 99,   // ✅ Add this catch-all
}
