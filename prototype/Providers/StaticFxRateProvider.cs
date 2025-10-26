using model.Domain.Values;
using model.Interfaces;

namespace model.Providers;
public class StaticFxRateProvider : IFxRateProvider
{
    private readonly Dictionary<(string, string, DateTime), FxRate> _rates;

    public StaticFxRateProvider(Dictionary<(string, string, DateTime), FxRate> rates)
    {
        _rates = rates;
    }

    public FxRate? GetRate(Currency fromCurrency, Currency toCurrency, DateTime date)
    {
        _rates.TryGetValue((fromCurrency.Code, toCurrency.Code, date), out var rate);
        return rate;
    }
}