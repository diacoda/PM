using model.Domain.Values;

namespace model.Interfaces;

public interface IFxRateProvider
{
    FxRate? GetRate(Currency fromCurrency, Currency toCurrency, DateTime date);
}
