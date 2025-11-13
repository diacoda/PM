using PM.DTO.Prices;

namespace PM.Application.Interfaces;

public interface IDailyPriceAggregator
{
    /// <summary>
    /// Returns an aggregated result for the given date after attempting fetches.
    /// Does not publish the domain event (that is done by the caller).
    /// </summary>
    Task<FetchPricesDTO> RunOnceAsync(DateOnly date, bool allowMarketClosed = false, CancellationToken ct = default);
}