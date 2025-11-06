using System;

namespace PM.Domain.Values
{
    /// <summary>
    /// Represents a foreign exchange (FX) rate between two currencies on a specific date.
    /// </summary>
    public record FxRate
    {
        /// <summary>
        /// Parameterless constructor required by EF Core.
        /// </summary>
        private FxRate() { }

        /// <summary>
        /// Creates a new FX rate instance for a currency pair on a given date.
        /// </summary>
        /// <param name="fromCurrency">The source currency.</param>
        /// <param name="toCurrency">The target currency.</param>
        /// <param name="date">The date of the rate.</param>
        /// <param name="rate">The exchange rate (target per one unit of source currency).</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="fromCurrency"/> or <paramref name="toCurrency"/> is null.</exception>
        public FxRate(Currency fromCurrency, Currency toCurrency, DateOnly date, decimal rate)
        {
            FromCurrency = fromCurrency ?? throw new ArgumentNullException(nameof(fromCurrency));
            ToCurrency = toCurrency ?? throw new ArgumentNullException(nameof(toCurrency));
            Date = date;
            Rate = rate;
        }

        /// <summary>
        /// Gets or sets the source currency of the FX rate.
        /// </summary>
        public Currency FromCurrency { get; set; } = default!;

        /// <summary>
        /// Gets or sets the target currency of the FX rate.
        /// </summary>
        public Currency ToCurrency { get; set; } = default!;

        /// <summary>
        /// Gets or sets the date of the FX rate.
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Gets or sets the exchange rate (amount of <see cref="ToCurrency"/> per one unit of <see cref="FromCurrency"/>).
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// Gets a string representing the currency pair in "FROM/TO" format.
        /// </summary>
        public string Pair => $"{FromCurrency}/{ToCurrency}";
    }
}
