using PM.Domain.Entities;
using PM.Domain.Values;


namespace PM.Application.Interfaces;

public interface IPricingService
{
    Task<Money> CalculateHoldingValueAsync(Holding holding, DateOnly date, Currency reportingCurrency, CancellationToken ct = default);
    Task<Money> CalculateAccountValueAsync(Account account, DateOnly date, Currency reportingCurrency, CancellationToken ct = default);
    Task<Money> CalculatePortfolioValueAsync(Portfolio portfolio, DateOnly date, Currency reportingCurrency, CancellationToken ct = default);
}