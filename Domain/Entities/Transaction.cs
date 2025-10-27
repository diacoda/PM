using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; private set; }

        public DateTime Date { get; set; }

        public TransactionType Type { get; set; }

        public Instrument Instrument { get; set; } = default!; // Owned type

        public decimal Quantity { get; set; }

        public Money Amount { get; set; } = default!; // Owned type

        public Money? Costs { get; set; } // Owned type, optional

        public int AccountId { get; set; }
        public Account? Account { get; set; }
    }
}
