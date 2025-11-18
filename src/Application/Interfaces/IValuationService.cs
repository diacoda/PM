using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IValuationService
{
    // Snapshot generators
    Task<Valuation> GeneratePortfolioValuationSnapshot(int portfolioId, DateOnly date, Currency reportingCurrency, CancellationToken ct = default);
    Task<ValuationSnapshot> GenerateAccountValuationSnapshot(int portfolioId, int accountId, DateOnly date, Currency reportingCurrency, CancellationToken ct = default);
    Task<IEnumerable<ValuationSnapshot>> GeneratePortfolioAssetClassValuationSnapshot(int portfolioId, DateOnly date, Currency reportingCurrency, CancellationToken ct = default);
    Task<IEnumerable<ValuationSnapshot>> GenerateAccountAssetClassValuationSnapshot(int portfolioId, int accountId, DateOnly date, Currency reportingCurrency, CancellationToken ct = default);

    // Storage methods
    Task StoreEstateValuation(ValuationSnapshot valuation, DateOnly date, ValuationPeriod period, CancellationToken ct = default);
    Task StoreEstateAssetClassValuation(IEnumerable<ValuationSnapshot> valuation, DateOnly date, ValuationPeriod period, CancellationToken ct = default);
    Task StoreOwnerValuation(string owner, ValuationSnapshot valuation, DateOnly date, ValuationPeriod period, CancellationToken ct = default);
    Task StoreOwnerAssetClassValuation(string owner, IEnumerable<ValuationSnapshot> valuation, DateOnly date, ValuationPeriod period, CancellationToken ct = default);

    Task StorePortfolioValuation(int portfolioId, Valuation valuation, DateOnly date, ValuationPeriod period, CancellationToken ct = default);
    Task StoreAccountValuation(int portfolioId, int accountId, ValuationSnapshot valuation, DateOnly date, ValuationPeriod period, CancellationToken ct = default);
    Task StorePortfolioAssetClassValuation(int portfolioId, IEnumerable<ValuationSnapshot> valuations, DateOnly date, ValuationPeriod period, CancellationToken ct = default);
    Task StoreAccountAssetClassValuation(int portfolioId, int accountId, IEnumerable<ValuationSnapshot> valuations, DateOnly date, ValuationPeriod period, CancellationToken ct = default);

    //READ
    Task<ValuationSnapshot?> GetLatestAsync(
            EntityKind kind,
            int entityId,
            Currency currency,
            ValuationPeriod? period = null,
            bool includeAssetClass = false,
            CancellationToken ct = default);

    Task<IEnumerable<ValuationSnapshot>> GetHistoryAsync(
        EntityKind kind,
        int entityId,
        DateOnly start,
        DateOnly end,
        Currency currency,
        ValuationPeriod? period = null,
        CancellationToken ct = default);

    Task<IEnumerable<ValuationSnapshot>> GetAsOfDateAsync(
        EntityKind kind,
        DateOnly date,
        Currency currency,
        ValuationPeriod? period = null,
        CancellationToken ct = default);
}