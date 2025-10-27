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
    Task<IEnumerable<ValuationRecord>> GetByPortfolioAsync(int portfolioId, ValuationPeriod period);
    // ---------------------------------------------------------------------
    // TOTAL SNAPSHOTS (Portfolio / Account)
    // ---------------------------------------------------------------------

    Task GenerateAndStorePortfolioValuations(
        Portfolio portfolio,
        DateTime startDate,
        DateTime endDate,
        Currency reportingCurrency,
        ValuationPeriod period);
    Task GenerateAndStoreAccountValuations(
        Account account,
        DateTime startDate,
        DateTime endDate,
        Currency reportingCurrency,
        ValuationPeriod period);
    // ---------------------------------------------------------------------
    // ASSET-CLASS SNAPSHOTS (Portfolio / Account) with Percentage
    // ---------------------------------------------------------------------
    Task GenerateAndStorePortfolioValuationsByAssetClass(
        Portfolio portfolio,
        DateTime startDate,
        DateTime endDate,
        Currency reportingCurrency,
        ValuationPeriod period);
    Task GenerateAndStoreAccountValuationsByAssetClass(
        Account account,
        DateTime startDate,
        DateTime endDate,
        Currency reportingCurrency,
        ValuationPeriod period);
}