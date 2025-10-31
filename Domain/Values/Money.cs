namespace PM.Domain.Values
{
    /// <summary>
    /// Represents an amount of money in a specific currency.
    /// </summary>
    public class Money
    {
        /// <summary>
        /// The monetary amount.
        /// </summary>
        public decimal Amount { get; private set; }

        /// <summary>
        /// The currency of the amount.
        /// </summary>
        public Currency Currency { get; private set; } = default!;

        /// <summary>
        /// Private constructor for EF Core or serialization.
        /// </summary>
        private Money() { }

        /// <summary>
        /// Creates a new instance of <see cref="Money"/> with a given amount and currency.
        /// </summary>
        /// <param name="amount">The monetary amount.</param>
        /// <param name="currency">The currency of the amount.</param>
        public Money(decimal amount, Currency currency)
        {
            Amount = amount;
            Currency = currency;
        }

        /// <summary>
        /// Returns a string representation of the money amount and currency.
        /// </summary>
        public override string ToString() => $"{Amount} {Currency}";
    }
}
