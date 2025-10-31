using PM.Domain.Enums;
using PM.Domain.Values;
using PM.DTO;
using PM.SharedKernel;

namespace PM.Domain.Entities;

public class Transaction : Entity
{
    public int Id { get; private set; }

    public DateTime Date { get; set; }

    public TransactionType Type { get; set; }

    public Symbol Symbol { get; set; } = default!; // Owned type

    public decimal Quantity { get; set; }

    public Money Amount { get; set; } = default!; // Owned type

    public Money? Costs { get; set; } // Owned type, optional

    public int AccountId { get; set; }
    public Account? Account { get; set; }

    public Transaction()
    {
    }

    public Transaction(
        int accountId,
        TransactionType type,
        Symbol instrument,
        decimal quantity,
        Money amount,
        DateTime date
    )
    {
        AccountId = accountId;
        Type = type;
        Symbol = instrument;
        Quantity = quantity;
        Amount = amount;
        Date = date;
    }
}
