using PM.Domain.Enums;

namespace PM.Application.Interfaces;

public interface IValuationCalculator
{
    /// <summary>
    /// Calculates valuations for all portfolios/accounts for a given date and stores them under all applicable periods.
    /// </summary>
    Task CalculateValuationsAsync(DateOnly date, IEnumerable<ValuationPeriod> periods, CancellationToken ct = default);
}
