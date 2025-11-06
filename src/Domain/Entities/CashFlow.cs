using PM.Domain.Enums;
using PM.Domain.Values;
using PM.SharedKernel;

namespace PM.Domain.Entities
{
    /// <summary>
    /// Represents a cash flow event (inflow or outflow) for an account.
    /// </summary>
    public class CashFlow : Entity
    {
        /// <summary>
        /// Gets or sets the unique identifier of the cash flow.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the foreign key of the account associated with this cash flow.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Gets or sets the date of the cash flow.
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Gets or sets the amount of money involved in the cash flow.
        /// </summary>
        public Money Amount { get; set; } = new Money(0.0m, Currency.CAD);

        /// <summary>
        /// Gets or sets the type of cash flow (e.g., deposit, withdrawal).
        /// </summary>
        public CashFlowType Type { get; set; }

        /// <summary>
        /// Gets or sets an optional note or description for the cash flow.
        /// </summary>
        public string? Note { get; set; }
    }
}
