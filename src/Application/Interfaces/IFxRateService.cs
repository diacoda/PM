using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IFxRateService
{
    /// <summary>
    /// Get from DB or fetch from provider the rate 
    /// </summary>
    /// <param name="fromCurrencyCode"></param>
    /// <param name="toCurrencyCode"></param>
    /// <param name="date"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<FxRate?> GetOrFetchRateAsync(string fromCurrencyCode, string toCurrencyCode, DateOnly date, CancellationToken ct = default);
    /// <summary>
    /// Insert or update an FX rate for a currency pair and date.
    /// </summary>
    Task<FxRate> UpdateRateAsync(string fromCurrencyCode, string toCurrencyCode, decimal rate, DateOnly date, CancellationToken ct = default);

    /// <summary>
    /// Get an FX rate for a currency pair on a specific date.
    /// </summary>
    Task<FxRate?> GetRateAsync(string fromCurrencyCode, string toCurrencyCode, DateOnly date, CancellationToken ct = default);

    /// <summary>
    /// Get all historical FX rates for a currency pair.
    /// </summary>
    Task<List<FxRate>> GetAllRatesForPairAsync(string fromCurrencyCode, string toCurrencyCode, CancellationToken ct = default);

    /// <summary>
    /// Delete an FX rate for a currency pair and date.
    /// </summary>
    Task<bool> DeleteRateAsync(string fromCurrencyCode, string toCurrencyCode, DateOnly date, CancellationToken ct = default);

    /// <summary>
    /// Get all FX rates for a given date.
    /// </summary>
    Task<List<FxRate>> GetAllRatesByDateAsync(DateOnly date, CancellationToken ct = default);
}
