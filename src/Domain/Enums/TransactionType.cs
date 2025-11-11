namespace PM.Domain.Enums;

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Buy,
    Sell,
    Dividend,
    Interest,
    Other = 99,   // ✅ Add this catch-all
}
