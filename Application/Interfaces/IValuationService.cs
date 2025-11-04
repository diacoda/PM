using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

public interface IValuationService
{
    Task<IEnumerable<ValuationRecord>> GetByPortfolioAsync(int portfolioId, ValuationPeriod period, CancellationToken ct = default);

    Task GenerateAndStorePortfolioValuations(
        int portfolioId,
        DateOnly date,
        Currency reportingCurrency,
        ValuationPeriod period,
        CancellationToken ct = default);

    Task GenerateAndStoreAccountValuations(
        int portfolioId,
        int accountId,
        DateOnly date,
        Currency reportingCurrency,
        ValuationPeriod period,
        CancellationToken ct = default);

    Task GenerateAndStorePortfolioValuationsByAssetClass(
        int portfolioId,
        DateOnly date,
        Currency reportingCurrency,
        ValuationPeriod period,
        CancellationToken ct = default);

    Task GenerateAndStoreAccountValuationsByAssetClass(
        int portfolioId,
        int accountId,
        DateOnly date,
        Currency reportingCurrency,
        ValuationPeriod period,
        CancellationToken ct = default);
}
