using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Domain.Enums;

namespace PM.Application.Interfaces;

/// <summary>
/// Produces and stores valuation snapshots using the existing ValuationService.
/// Adds per-snapshot component breakdown (SecuritiesValue, CashValue, IncomeForDay).
/// Also persists Asset-Class slices with Percentage weights.
/// </summary>
public interface IValuationService
{
    Task<IEnumerable<ValuationRecord>> GetByPortfolioAsync(int portfolioId, ValuationPeriod period, CancellationToken ct = default);
    // ---------------------------------------------------------------------
    // TOTAL SNAPSHOTS (Portfolio / Account)
    // ---------------------------------------------------------------------

    Task GenerateAndStorePortfolioValuations(
        int portfolioId,
        DateTime startDate,
        DateTime endDate,
        Currency reportingCurrency,
        ValuationPeriod period, CancellationToken ct = default);
    Task GenerateAndStoreAccountValuations(
        int portfolioId,
        int accountId,
        DateTime startDate,
        DateTime endDate,
        Currency reportingCurrency,
        ValuationPeriod period, CancellationToken ct = default);
    // ---------------------------------------------------------------------
    // ASSET-CLASS SNAPSHOTS (Portfolio / Account) with Percentage
    // ---------------------------------------------------------------------
    Task GenerateAndStorePortfolioValuationsByAssetClass(
        int portfolioId,
        DateTime startDate,
        DateTime endDate,
        Currency reportingCurrency,
        ValuationPeriod period, CancellationToken ct = default);
    Task GenerateAndStoreAccountValuationsByAssetClass(
        int portfolioId,
        int accountId,
        DateTime startDate,
        DateTime endDate,
        Currency reportingCurrency,
        ValuationPeriod period,
        CancellationToken ct = default);
}