using model.Domain.Entities;
using model.Domain.Values;
using model.Interfaces;

namespace model.Services;
public class ValuationService
{

    private readonly IPriceProvider _priceProvider;
    private readonly IFxRateProvider _fxRateProvider;

    public ValuationService(IPriceProvider priceProvider, IFxRateProvider fxRateProvider)
    {
        _priceProvider = priceProvider;
        _fxRateProvider = fxRateProvider;
    }
    
    public Money CalculateHoldingValue(Holding holding, DateTime date, Currency reportingCurrency)
    {
        var price = _priceProvider.GetPrice(holding.Instrument.Symbol, date);
        if (price == null) return new Money(0, reportingCurrency);

        FxRate? fx = null;
        if (price.Price.Currency != reportingCurrency)
        {
            fx = _fxRateProvider.GetRate(price.Price.Currency, reportingCurrency, date);
        }

        decimal value = holding.Quantity * price.Price.Amount;
        if (fx != null)
        {
            value *= fx.Rate;
        }

        return new Money(value, reportingCurrency);
    }

    public Money CalculateAccountValue(Account account, DateTime date, Currency reportingCurrency)
    {
        decimal total = 0m;

        foreach (var holding in account.Holdings)
        {
            var value = CalculateHoldingValue(holding, date, reportingCurrency);
            total += value.Amount;
        }

        return new Money(total, reportingCurrency);
    }

    public Money CalculatePortfolioValue(Portfolio portfolio, DateTime date, Currency reportingCurrency)
    {
        decimal total = 0m;

        foreach (var account in portfolio.Accounts)
        {
            var accountValue = CalculateAccountValue(account, date, reportingCurrency);
            total += accountValue.Amount;
        }

        return new Money(total, reportingCurrency);
    }
}