using PM.Domain.Values;
namespace PM.Application.Interfaces;

public interface IFxRateRepository
{
    Task<FxRate?> GetAsync(Currency fromCurrency, Currency toCurrency, DateOnly date);
    Task SaveAsync(FxRate rate);
    Task UpsertAsync(FxRate rate);
    Task<List<FxRate>> GetAllForPairAsync(Currency fromCurrency, Currency toCurrency);
    Task<bool> DeleteAsync(Currency fromCurrency, Currency toCurrency, DateOnly date);
    Task<List<FxRate>> GetAllByDateAsync(DateOnly date);
}