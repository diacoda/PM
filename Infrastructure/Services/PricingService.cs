using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Application.Interfaces;

namespace PM.Infrastructure.Services;

public class PricingService : IPricingService
{

    private readonly IPriceProvider _priceProvider;
    private readonly IFxRateProvider _fxRateProvider;

    public PricingService(IPriceProvider priceProvider, IFxRateProvider fxRateProvider)
    {
        _priceProvider = priceProvider;
        _fxRateProvider = fxRateProvider;
    }

    public async Task<Money> CalculateHoldingValueAsync(Holding holding, DateTime date, Currency reportingCurrency, CancellationToken ct = default)
    {
        var price = await _priceProvider.GetPriceAsync(holding.Symbol, DateOnly.FromDateTime(date), ct);
        if (price == null) return new Money(0, reportingCurrency);

        FxRate? fx = null;
        if (price.Price.Currency != reportingCurrency)
        {
            fx = await _fxRateProvider.GetFxRateAsync(price.Price.Currency, reportingCurrency, DateOnly.FromDateTime(date), ct);
        }

        decimal value = holding.Quantity * price.Price.Amount;
        if (fx != null)
        {
            value *= fx.Rate;
        }

        return new Money(value, reportingCurrency);
    }

    public async Task<Money> CalculateAccountValueAsync(Account account, DateTime date, Currency reportingCurrency, CancellationToken ct = default)
    {
        decimal total = 0m;

        foreach (var holding in account.Holdings)
        {
            var value = await CalculateHoldingValueAsync(holding, date, reportingCurrency, ct);
            total += value.Amount;
        }

        return new Money(total, reportingCurrency);
    }

    public async Task<Money> CalculatePortfolioValueAsync(Portfolio portfolio, DateTime date, Currency reportingCurrency, CancellationToken ct = default)
    {
        decimal total = 0m;

        foreach (var account in portfolio.Accounts)
        {
            var accountValue = await CalculateAccountValueAsync(account, date, reportingCurrency, ct);
            total += accountValue.Amount;
        }

        return new Money(total, reportingCurrency);
    }
}