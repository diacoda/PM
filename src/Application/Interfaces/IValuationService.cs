using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IValuationService
{
    // Snapshot generators
    Task<ValuationRecord> GeneratePortfolioValuationSnapshot(int portfolioId, DateOnly date, Currency reportingCurrency, CancellationToken ct = default);
    Task<ValuationRecord> GenerateAccountValuationSnapshot(int portfolioId, int accountId, DateOnly date, Currency reportingCurrency, CancellationToken ct = default);
    Task<IEnumerable<ValuationRecord>> GeneratePortfolioAssetClassValuationSnapshot(int portfolioId, DateOnly date, Currency reportingCurrency, CancellationToken ct = default);
    Task<IEnumerable<ValuationRecord>> GenerateAccountAssetClassValuationSnapshot(int portfolioId, int accountId, DateOnly date, Currency reportingCurrency, CancellationToken ct = default);

    // Storage methods
    Task StorePortfolioValuation(int portfolioId, ValuationRecord valuation, DateOnly date, ValuationPeriod period, CancellationToken ct = default);
    Task StoreAccountValuation(int portfolioId, int accountId, ValuationRecord valuation, DateOnly date, ValuationPeriod period, CancellationToken ct = default);
    Task StorePortfolioAssetClassValuation(int portfolioId, IEnumerable<ValuationRecord> valuations, DateOnly date, ValuationPeriod period, CancellationToken ct = default);
    Task StoreAccountAssetClassValuation(int portfolioId, int accountId, IEnumerable<ValuationRecord> valuations, DateOnly date, ValuationPeriod period, CancellationToken ct = default);

    //READ
    Task<ValuationRecord?> GetLatestAsync(
            EntityKind kind,
            int entityId,
            Currency currency,
            ValuationPeriod? period = null,
            bool includeAssetClass = false,
            CancellationToken ct = default);

    Task<IEnumerable<ValuationRecord>> GetHistoryAsync(
        EntityKind kind,
        int entityId,
        DateOnly start,
        DateOnly end,
        Currency currency,
        ValuationPeriod? period = null,
        CancellationToken ct = default);

    Task<IEnumerable<ValuationRecord>> GetAsOfDateAsync(
        EntityKind kind,
        DateOnly date,
        Currency currency,
        ValuationPeriod? period = null,
        CancellationToken ct = default);
}