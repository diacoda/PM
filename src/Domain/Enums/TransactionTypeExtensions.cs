namespace PM.Domain.Enums;

public static class TransactionTypeExtensions
{
    public static bool IsIncome(this TransactionType type) =>
        type == TransactionType.Dividend || type == TransactionType.Interest;
}