using PM.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PM.Application.Interfaces
{
    /// <summary>
    /// Defines the valuation calculator functionality.
    /// Can calculate valuations for all portfolios/accounts for a given date and period.
    /// </summary>
    public interface IValuationCalculator
    {
        /// <summary>
        /// Calculate valuations for all portfolios/accounts for a given date and period.
        /// </summary>
        /// <param name="date">The date to calculate valuation for.</param>
        /// <param name="period">The valuation period (daily, monthly, etc.).</param>
        /// <param name="ct">Cancellation token.</param>
        Task CalculateValuationAsync(DateTime date, ValuationPeriod period, CancellationToken ct = default);
    }
}
