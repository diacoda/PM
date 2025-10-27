using PM.Domain.Values;
using PM.Application.Interfaces;

namespace PM.Infrastructure.Providers;

public class DynamicPriceProvider : IPriceProvider
{
    private static Currency DetermineCurrency(Symbol symbol)
    {
        var code = symbol.Value.ToUpperInvariant();
        {
            if (code.StartsWith("CASH."))        // e.g., CASH.CAD / CASH.USD
            {
                var c = code.Split('.')[1];
                return new Currency(c);
            }
        }
        if (code.EndsWith(".TO")) return new Currency("CAD"); // TSX tickers
        if (code.EndsWith(".CAD")) return new Currency("CAD");
        if (code.EndsWith(".USD")) return new Currency("USD");
        // Simple heuristic for demo
        return code is "VOO" or "USBOND" ? new Currency("USD") : new Currency("CAD");
    }

    public InstrumentPrice? GetPrice(Symbol symbol, DateTime date)
    {
        var ccy = DetermineCurrency(symbol);
        // Cash is always 1 in its currency
        if (symbol.Value.StartsWith("CASH.", StringComparison.OrdinalIgnoreCase))
            return new InstrumentPrice(symbol, date.Date, new Money(1m, ccy));

        // Deterministic-ish per (symbol, date) within a single run
        var seed = StableSeed($"{symbol.Value}|{date.Date:yyyyMMdd}");
        var rng = new Random(seed);
        var price = 80m + (decimal)rng.NextDouble() * 40m; // [80,120)
        return new InstrumentPrice(symbol, date.Date, new Money(decimal.Round(price, 2), ccy));
    }

    private static int StableSeed(string key)
    {
        unchecked
        {
            int h = 17;
            foreach (var ch in key)
                h = h * 31 + ch;
            return h;
        }
    }
}
