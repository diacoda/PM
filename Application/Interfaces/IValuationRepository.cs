using PM.Domain.Entities;
using PM.Domain.Enums;

namespace PM.Application.Interfaces;

public interface IValuationRepository
{
    Task SaveAsync(ValuationRecord record);
    Task<IEnumerable<ValuationRecord>> GetAllAsync();

    Task<IEnumerable<ValuationRecord>> GetByPortfolioAsync(int portfolioId, ValuationPeriod period);
    Task<IEnumerable<ValuationRecord>> GetByAccountAsync(int accountId, ValuationPeriod period);

    Task<IEnumerable<ValuationRecord>> GetPortfolioAssetClassSnapshotsAsync(
        int portfolioId, ValuationPeriod period, DateTime? from = null, DateTime? to = null);

    Task<IEnumerable<ValuationRecord>> GetAccountAssetClassSnapshotsAsync(
        int accountId, ValuationPeriod period, DateTime? from = null, DateTime? to = null);
}
