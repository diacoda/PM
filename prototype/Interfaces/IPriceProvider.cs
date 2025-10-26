using model.Domain.Values;

namespace model.Interfaces;

public interface IPriceProvider
{
    InstrumentPrice? GetPrice(Symbol symbol, DateTime date);
}
