using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.DTO;
using PM.SharedKernel;

namespace PM.Domain.Entities
{
    /// <summary>
    /// Represents a financial transaction in an account, such as a buy, sell, or dividend.
    /// </summary>
    public class Transaction : Entity
    {
        /// <summary>
        /// Gets the unique identifier of the transaction.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        /// <summary>
        /// Gets or sets the date of the transaction.
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Gets or sets the type of the transaction (e.g., Buy, Sell, Dividend).
        /// </summary>
        public TransactionType Type { get; set; }

        /// <summary>
        /// Gets or sets the symbol of the asset involved in the transaction.
        /// </summary>
        public Symbol Symbol { get; set; } = default!;

        /// <summary>
        /// Gets or sets the quantity of the asset involved.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the total amount of the transaction in a specific currency.
        /// </summary>
        public Money Amount { get; set; } = default!;

        /// <summary>
        /// Gets or sets optional additional costs associated with the transaction, such as fees.
        /// </summary>
        public Money? Costs { get; set; }

        /// <summary>
        /// Gets or sets the foreign key of the account associated with this transaction.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Gets or sets the account that this transaction belongs to.
        /// </summary>
        public Account? Account { get; set; }

        /// <summary>
        /// EF Core parameterless constructor for entity materialization.
        /// </summary>
        public Transaction() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction"/> class with the specified details.
        /// </summary>
        /// <param name="accountId">The ID of the account where the transaction occurred.</param>
        /// <param name="type">The type of the transaction.</param>
        /// <param name="instrument">The symbol of the asset involved.</param>
        /// <param name="quantity">The quantity of the asset involved.</param>
        /// <param name="amount">The total amount of the transaction.</param>
        /// <param name="date">The date of the transaction.</param>
        public Transaction(
            int accountId,
            TransactionType type,
            Symbol instrument,
            decimal quantity,
            Money amount,
            DateOnly date)
        {
            AccountId = accountId;
            Type = type;
            Symbol = instrument;
            Quantity = quantity;
            Amount = amount;
            Date = date;
        }

        /// <summary>
        /// Returns a string representation of the transaction.
        /// </summary>
        /// <returns>A string showing the type, symbol, quantity, and amount.</returns>
        public override string ToString() =>
            $"{Type} {Quantity} {Symbol.Code} for {Amount} on {Date:d}";
    }
}
