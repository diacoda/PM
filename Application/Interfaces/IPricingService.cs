using PM.Domain.Entities;
using PM.Domain.Values;


namespace PM.Application.Interfaces;

public interface IPricingService
{
    Task<Money> CalculateHoldingValueAsync(Holding holding, DateTime date, Currency reportingCurrency);
    Task<Money> CalculateAccountValueAsync(Account account, DateTime date, Currency reportingCurrency);
    Task<Money> CalculatePortfolioValueAsync(Portfolio portfolio, DateTime date, Currency reportingCurrency);
}