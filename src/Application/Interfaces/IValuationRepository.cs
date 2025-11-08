using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IValuationRepository
{
    Task SaveAsync(ValuationSnapshot record, CancellationToken ct = default);
    Task<IEnumerable<ValuationSnapshot>> GetAllAsync(CancellationToken ct = default);

    Task<IEnumerable<ValuationSnapshot>> GetByPortfolioAsync(
        int portfolioId,
        ValuationPeriod period,
        CancellationToken ct = default);
    Task<IEnumerable<ValuationSnapshot>> GetByAccountAsync(
        int accountId,
        ValuationPeriod period,
        CancellationToken ct = default);

    Task<IEnumerable<ValuationSnapshot>> GetPortfolioAssetClassSnapshotsAsync(
        int portfolioId,
        ValuationPeriod period,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken ct = default);

    Task<IEnumerable<ValuationSnapshot>> GetAccountAssetClassSnapshotsAsync(
        int accountId,
        ValuationPeriod period,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken ct = default);

    Task<ValuationSnapshot?> GetLatestAsync(
            EntityKind kind,
            int entityId,
            Currency reportingCurrency,
            ValuationPeriod? period = null,
            bool includeAssetClass = false,
            CancellationToken ct = default);

    Task<IEnumerable<ValuationSnapshot>> GetRangeAsync(
        EntityKind kind,
        int entityId,
        DateOnly start,
        DateOnly end,
        Currency reportingCurrency,
        ValuationPeriod? period = null,
        AssetClass? assetClass = null,
        CancellationToken ct = default);

    Task<IEnumerable<ValuationSnapshot>> GetAsOfDateAsync(
        EntityKind kind,
        DateOnly date,
        Currency reportingCurrency,
        ValuationPeriod? period = null,
        CancellationToken ct = default);
}
