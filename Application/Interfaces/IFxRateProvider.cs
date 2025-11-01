using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IFxRateProvider
{
    string ProviderName { get; }
    Task<FxRate?> GetFxRateAsync(Currency fromCurrency, Currency toCurrency, DateOnly date, CancellationToken ct = default);
}
