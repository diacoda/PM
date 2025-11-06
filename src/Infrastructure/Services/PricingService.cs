using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Application.Interfaces;
using PM.Domain.Mappers;

namespace PM.Infrastructure.Services;

public class PricingService : IPricingService
{

    private readonly IPriceService _priceService;
    private readonly IFxRateService _fxRateService;

    public PricingService(IPriceService priceService, IFxRateService fxRateService)
    {
        _priceService = priceService;
        _fxRateService = fxRateService;
    }

    public async Task<Money> CalculateHoldingValueAsync(
        Holding holding,
        DateOnly date,
        Currency reportingCurrency,
        CancellationToken ct = default)
    {
        var symbolCode = holding.Asset.Code;
        var holdingCurrency = new Currency(holding.Asset.Currency.Code);

        decimal priceAmount;
        FxRate? fx = null;

        // 🪙 1️⃣ Handle cash positions (CAD or USD)
        if (symbolCode is "CAD" or "USD")
        {
            // 1 CAD = 1 CAD, but USD may need FX conversion
            priceAmount = 1m;

            if (holdingCurrency != reportingCurrency)
            {
                fx = await _fxRateService.GetRateAsync(
                    holdingCurrency.Code,
                    reportingCurrency.Code,
                    date,
                    ct);
            }
        }
        else
        {
            // 🧾 2️⃣ Handle non-cash instruments (equities, ETFs, etc.)
            var price = await _priceService.GetOrFetchInstrumentPriceAsync(symbolCode, date, ct);
            if (price is null)
                return new Money(0, reportingCurrency);

            priceAmount = price.Price.Amount;

            if (price.Price.Currency != reportingCurrency)
            {
                fx = await _fxRateService.GetRateAsync(
                    price.Price.Currency.Code,
                    reportingCurrency.Code,
                    date,
                    ct);
            }
        }

        // 💰 3️⃣ Calculate total value in reporting currency
        decimal value = holding.Quantity * priceAmount;

        if (fx is not null)
            value *= fx.Rate;

        return new Money(value, reportingCurrency);
    }


    public async Task<Money> CalculateAccountValueAsync(Account account, DateOnly date, Currency reportingCurrency, CancellationToken ct = default)
    {
        decimal total = 0m;

        foreach (var holding in account.Holdings)
        {
            var value = await CalculateHoldingValueAsync(holding, date, reportingCurrency, ct);
            total += value.Amount;
        }

        return new Money(total, reportingCurrency);
    }

    public async Task<Money> CalculatePortfolioValueAsync(Portfolio portfolio, DateOnly date, Currency reportingCurrency, CancellationToken ct = default)
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