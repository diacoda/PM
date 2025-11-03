using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.DTO;

namespace PM.Domain.Mappers;

public static class TransactionMapper
{
    public static TransactionDTO ToDTO(Transaction tx)
    {
        return new TransactionDTO
        {
            Id = tx.Id,
            Date = tx.Date,
            Type = tx.Type.ToString(),
            Symbol = tx.Symbol.Code,
            Quantity = tx.Quantity,
            Amount = tx.Amount.Amount,
            AmountCurrency = tx.Amount.Currency.Code,
            Costs = tx.Costs?.Amount,
            CostsCurrency = tx.Costs?.Currency.Code,
            AccountId = tx.AccountId
        };
    }
    public static Transaction ToEntity(TransactionDTO dto)
    {
        return new Transaction
        {
            AccountId = dto.AccountId,
            Date = dto.Date,
            Type = Enum.Parse<TransactionType>(dto.Type),
            Symbol = new Symbol(dto.Symbol),
            Quantity = dto.Quantity,
            Amount = new Money(dto.Amount, new Currency(dto.AmountCurrency)),
            Costs = new Money(dto.Costs ?? 0, new Currency(dto.CostsCurrency ?? "CAD")),
        };
    }

    public static Transaction ToEntity(int accountId, CreateTransactionDTO dto)
    {
        return new Transaction
        {
            AccountId = accountId,
            Date = dto.Date ?? DateTime.UtcNow,
            Type = Enum.Parse<TransactionType>(dto.Type),
            Symbol = new Symbol(dto.Symbol),
            Quantity = dto.Quantity,
            Amount = new Money(dto.Amount, new Currency(dto.AmountCurrency)),
            Costs = new Money(dto.Costs ?? 0, new Currency(dto.CostsCurrency ?? "CAD")),
        };
    }

    public static Transaction ToEntity(int accountId, TransactionType type, CashFlowDTO dto)
    {
        return new Transaction
        {
            AccountId = accountId,
            Date = dto.Date ?? DateTime.UtcNow,
            Type = type,
            Symbol = new Symbol(dto.Currency),
            Quantity = dto.Amount,
            Amount = new Money(dto.Amount, new Currency(dto.Currency)),
            Costs = new Money(0, new Currency(dto.Currency ?? "CAD")),
        };
    }

}
