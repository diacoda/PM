using PM.DTO.Prices;
using PM.Application.Interfaces;
using PM.Domain.Values;

namespace PM.Application.Commands;

public class FetchDailyPricesCommand
{
    private readonly IEnumerable<IPriceProvider> _providers;
    private readonly IPriceRepository _priceRepository;
    private readonly IMarketCalendar _calendar;
    private readonly List<Symbol> _symbols;
    private readonly IFxRateProvider _fxProvider;
    private readonly IFxRateRepository _fxRepository;

    private static readonly Currency[] Currencies =
        { new("EUR"), new("USD"), new("CAD") };

    public FetchDailyPricesCommand(
        IEnumerable<IPriceProvider> providers,
        IPriceRepository priceRepository,
        IMarketCalendar calendar,
        List<Symbol> symbols,
        IFxRateProvider fxProvider,
        IFxRateRepository fxRepository)
    {
        _providers = providers;
        _priceRepository = priceRepository;
        _calendar = calendar;
        _symbols = symbols;
        _fxProvider = fxProvider;
        _fxRepository = fxRepository;
    }

    /// <summary>
    /// Fetch prices for symbols and FX rates for all currency pairs.
    /// </summary>
    public async Task<FetchPricesDTO> ExecuteAsync(DateOnly date, bool allowMarketClosed = false, CancellationToken ct = default)
    {
        var fetched = new List<string>();
        var skipped = new List<string>();
        var errors = new List<string>();

        // --------------------------
        // Fetch equity prices
        // --------------------------
        foreach (var symbol in _symbols)
        {
            var exchange = symbol.Exchange;
            var marketOpen = _calendar.IsMarketOpen(date, exchange);

            if (!marketOpen && !allowMarketClosed)
            {
                skipped.Add($"{symbol.Value} ({exchange}) - market closed");
                continue;
            }

            if (date == DateOnly.FromDateTime(DateTime.Today) && !_calendar.IsAfterMarketClose(exchange))
            {
                skipped.Add($"{symbol.Value} ({exchange}) - before market close");
                continue;
            }

            var existing = await _priceRepository.GetAsync(symbol, date);
            if (existing != null)
            {
                skipped.Add($"{symbol.Value} ({exchange}) - already in DB");
                continue;
            }

            string providerName = symbol.Value.EndsWith(".TO", StringComparison.OrdinalIgnoreCase)
                ? "Yahoo"
                : "Investing";

            var provider = _providers.SingleOrDefault(p =>
                p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

            if (provider == null)
            {
                errors.Add($"{symbol.Value} ({exchange}) - no provider {providerName}");
                continue;
            }

            try
            {
                var price = await provider.GetPriceAsync(symbol, date, ct);
                if (price != null)
                {
                    var toSave = price with { Source = provider.ProviderName };
                    await _priceRepository.SaveAsync(toSave, ct);
                    fetched.Add($"{symbol.Value} ({exchange}) from {providerName}");
                }
                else
                {
                    skipped.Add($"{symbol.Value} ({exchange}) - provider returned null");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{symbol.Value} ({exchange}) - fetch failed: {ex.Message}");
            }
        }

        // --------------------------
        // Fetch FX rates
        // --------------------------
        var fxPairs = Currencies.SelectMany(c1 => Currencies, (from, to) => (from, to))
                                .Where(p => p.from != p.to);

        foreach (var (from, to) in fxPairs)
        {
            try
            {
                var existing = await _fxRepository.GetAsync(from, to, date, ct);
                if (existing != null)
                {
                    skipped.Add($"FX {from.Code}/{to.Code} - already in DB");
                    continue;
                }

                var fx = await _fxProvider.GetFxRateAsync(from, to, date, ct);
                if (fx != null)
                {
                    await _fxRepository.SaveAsync(fx, ct);
                    fetched.Add($"FX {from.Code}/{to.Code}");
                }
                else
                {
                    skipped.Add($"FX {from.Code}/{to.Code} - provider returned null");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"FX {from.Code}/{to.Code} - fetch failed: {ex.Message}");
            }
        }

        return new FetchPricesDTO(date, fetched, skipped, errors);
    }
}
