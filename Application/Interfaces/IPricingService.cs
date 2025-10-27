using PM.Domain.Entities;
using PM.Domain.Values;


namespace PM.Application.Interfaces;

public interface IPricingService
{
    Money CalculateHoldingValue(Holding holding, DateTime date, Currency reportingCurrency);
    Money CalculateAccountValue(Account account, DateTime date, Currency reportingCurrency);
    Money CalculatePortfolioValue(Portfolio portfolio, DateTime date, Currency reportingCurrency);
}