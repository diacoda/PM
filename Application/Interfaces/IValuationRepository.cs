using PM.Domain.Entities;
using PM.Domain.Enums;

namespace PM.Application.Interfaces;

public interface IValuationRepository
{
    Task SaveAsync(ValuationRecord record, CancellationToken ct = default);
    Task<IEnumerable<ValuationRecord>> GetAllAsync(CancellationToken ct = default);

    Task<IEnumerable<ValuationRecord>> GetByPortfolioAsync(int portfolioId, ValuationPeriod period, CancellationToken ct = default);
    Task<IEnumerable<ValuationRecord>> GetByAccountAsync(int accountId, ValuationPeriod period, CancellationToken ct = default);

    Task<IEnumerable<ValuationRecord>> GetPortfolioAssetClassSnapshotsAsync(
        int portfolioId, ValuationPeriod period, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);

    Task<IEnumerable<ValuationRecord>> GetAccountAssetClassSnapshotsAsync(
        int accountId, ValuationPeriod period, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
}
