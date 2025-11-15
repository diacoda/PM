using PM.Application.Interfaces;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.SharedKernel;

namespace PM.Application.Services;
public class ValuationCalculator : IValuationCalculator
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IValuationService _valuationService;

    public ValuationCalculator(IPortfolioRepository portfolioRepository, IValuationService valuationService)
    {
        _portfolioRepository = portfolioRepository;
        _valuationService = valuationService;
    }

    public async Task CalculateAccountValuationAsync(DateOnly date, int portfolioId, int accountId, IEnumerable<ValuationPeriod> periods, CancellationToken ct = default)
    {
        
    }

    public async Task CalculateValuationsAsync(DateOnly date, IEnumerable<ValuationPeriod> periods, CancellationToken ct = default)
    {
        var portfolios = await _portfolioRepository.ListWithIncludesAsync(
            new[] { IncludeOption.Accounts, IncludeOption.Holdings }, ct);

        //var dateOnly = DateOnly.FromDateTime(date);
        var reportingCurrency = Currency.CAD;
        foreach (var portfolio in portfolios)
        {
            // Compute portfolio snapshot once
            var portfolioValuation = await _valuationService.GeneratePortfolioValuationSnapshot(
                portfolio.Id, date, reportingCurrency, ct);

            foreach (var period in periods)
            {
                await _valuationService.StorePortfolioValuation(portfolio.Id, portfolioValuation, date, period, ct);
            }

            // Accounts
            foreach (var account in portfolio.Accounts)
            {
                var accountValuation = await _valuationService.GenerateAccountValuationSnapshot(
                    portfolio.Id, account.Id, date, reportingCurrency, ct);

                foreach (var period in periods)
                {
                    await _valuationService.StoreAccountValuation(portfolio.Id, account.Id, accountValuation, date, period, ct);
                }

                var accountAssetClassValuation = await _valuationService.GenerateAccountAssetClassValuationSnapshot(
                    portfolio.Id, account.Id, date, reportingCurrency, ct);

                foreach (var period in periods)
                {
                    await _valuationService.StoreAccountAssetClassValuation(portfolio.Id, account.Id, accountAssetClassValuation, date, period, ct);
                }
            }

            // Portfolio Asset Class
            var portfolioAssetClassValuation = await _valuationService.GeneratePortfolioAssetClassValuationSnapshot(
                portfolio.Id, date, reportingCurrency, ct);

            foreach (var period in periods)
            {
                await _valuationService.StorePortfolioAssetClassValuation(portfolio.Id, portfolioAssetClassValuation, date, period, ct);
            }
        }
    }
}