using PM.Domain.Values;

namespace PM.Domain.Interfaces
{
    /// <summary>
    /// Represents a financial asset with a code, currency, and asset class.
    /// </summary>
    public interface IAsset
    {
        /// <summary>
        /// Gets the unique code identifying the asset (e.g., ticker symbol).
        /// </summary>
        string Code { get; }

        /// <summary>
        /// Gets the currency in which the asset is denominated.
        /// </summary>
        Currency Currency { get; }

        /// <summary>
        /// Gets the classification of the asset (e.g., equity, cash, commodity).
        /// </summary>
        AssetClass AssetClass { get; }
    }
}
