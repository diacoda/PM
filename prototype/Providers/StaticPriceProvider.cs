using model.Domain.Values;
using model.Interfaces;

namespace model.Providers;
public class StaticPriceProvider : IPriceProvider
{
    private readonly Dictionary<(string, DateTime), InstrumentPrice> _prices;

    public StaticPriceProvider(Dictionary<(string, DateTime), InstrumentPrice> prices)
    {
        _prices = prices;
    }

    public InstrumentPrice? GetPrice(Symbol symbol, DateTime date)
    {
        _prices.TryGetValue((symbol.Code, date), out var price);
        return price;
    }
}