using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IValuationRepository
{
    Task SaveAsync(ValuationRecord record, CancellationToken ct = default);
    Task<IEnumerable<ValuationRecord>> GetAllAsync(CancellationToken ct = default);

    Task<IEnumerable<ValuationRecord>> GetByPortfolioAsync(
        int portfolioId,
        ValuationPeriod period,
        CancellationToken ct = default);
    Task<IEnumerable<ValuationRecord>> GetByAccountAsync(
        int accountId,
        ValuationPeriod period,
        CancellationToken ct = default);

    Task<IEnumerable<ValuationRecord>> GetPortfolioAssetClassSnapshotsAsync(
        int portfolioId,
        ValuationPeriod period,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken ct = default);

    Task<IEnumerable<ValuationRecord>> GetAccountAssetClassSnapshotsAsync(
        int accountId,
        ValuationPeriod period,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken ct = default);

    Task<ValuationRecord?> GetLatestAsync(
            EntityKind kind,
            int entityId,
            Currency reportingCurrency,
            ValuationPeriod? period = null,
            bool includeAssetClass = false,
            CancellationToken ct = default);

    Task<IEnumerable<ValuationRecord>> GetRangeAsync(
        EntityKind kind,
        int entityId,
        DateOnly start,
        DateOnly end,
        Currency reportingCurrency,
        ValuationPeriod? period = null,
        AssetClass? assetClass = null,
        CancellationToken ct = default);

    Task<IEnumerable<ValuationRecord>> GetAsOfDateAsync(
        EntityKind kind,
        DateOnly date,
        Currency reportingCurrency,
        ValuationPeriod? period = null,
        CancellationToken ct = default);
}
