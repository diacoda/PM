using model.Domain.Values;
using model.Interfaces;

namespace model.Providers;

public class DynamicFxRateProvider : IFxRateProvider
{
    public FxRate? GetRate(Currency fromCurrency, Currency toCurrency, DateTime date)
    {
        if (fromCurrency.Code.Equals(toCurrency.Code, StringComparison.OrdinalIgnoreCase))
            return new FxRate(fromCurrency, toCurrency, date.Date, 1m);

        var key = $"{fromCurrency.Code}->{toCurrency.Code}|{date.Date:yyyyMMdd}";
        var rng = new Random(StableSeed(key));
        var rate = 1.0m + (decimal)rng.NextDouble() * 0.30m; // [1.00, 1.30)
        return new FxRate(fromCurrency, toCurrency, date.Date, decimal.Round(rate, 4));
    }

    private static int StableSeed(string key)
    {
        unchecked
        {
            int h = 23;
            foreach (var ch in key)
                h = h * 31 + ch;
            return h;
        }
    }
}