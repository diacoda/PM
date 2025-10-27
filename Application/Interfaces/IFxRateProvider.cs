using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IFxRateProvider
{
    FxRate? GetRate(Currency fromCurrency, Currency toCurrency, DateTime date);
}
