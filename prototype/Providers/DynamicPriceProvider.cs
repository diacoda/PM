using model.Domain.Values;
using model.Interfaces;

namespace model.Providers;

public class DynamicPriceProvider : IPriceProvider
{
    private static Currency DetermineCurrency(Symbol symbol)
    {
        var code = symbol.Code.ToUpperInvariant();
        if (code.StartsWith("CASH."))        // e.g., CASH.CAD / CASH.USD
        {
            var c = code.Split('.')[1];
            return Currency.From(c);
        }
        if (code.EndsWith(".TO")) return Currency.From("CAD"); // TSX tickers
        if (code.EndsWith(".CAD")) return Currency.From("CAD");
        if (code.EndsWith(".USD")) return Currency.From("USD");
        // Simple heuristic for demo
        return code is "VOO" or "USBOND" ? Currency.From("USD") : Currency.From("CAD");
    }

    public InstrumentPrice? GetPrice(Symbol symbol, DateTime date)
    {
        var ccy = DetermineCurrency(symbol);
        // Cash is always 1 in its currency
        if (symbol.Code.StartsWith("CASH.", StringComparison.OrdinalIgnoreCase))
            return new InstrumentPrice(symbol, date.Date, new Money(1m, ccy));

        // Deterministic-ish per (symbol, date) within a single run
        var seed = StableSeed($"{symbol.Code}|{date.Date:yyyyMMdd}");
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
