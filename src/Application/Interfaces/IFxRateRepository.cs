using PM.Domain.Values;
namespace PM.Application.Interfaces;

public interface IFxRateRepository
{
    Task<FxRate?> GetAsync(Currency fromCurrency, Currency toCurrency, DateOnly date, CancellationToken ct = default);
    Task SaveAsync(FxRate rate, CancellationToken ct = default);
    Task UpsertAsync(FxRate rate, CancellationToken ct = default);
    Task<List<FxRate>> GetAllForPairAsync(Currency fromCurrency, Currency toCurrency, CancellationToken ct = default);
    Task<bool> DeleteAsync(Currency fromCurrency, Currency toCurrency, DateOnly date, CancellationToken ct = default);
    Task<List<FxRate>> GetAllByDateAsync(DateOnly date, CancellationToken ct = default);
}