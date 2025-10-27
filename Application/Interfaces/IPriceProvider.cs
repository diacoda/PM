using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IPriceProvider
{
    InstrumentPrice? GetPrice(Symbol symbol, DateTime date);
}
