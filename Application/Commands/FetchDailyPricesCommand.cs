using PM.DTO.Prices;
using PM.Application.Interfaces;
using PM.Domain.Values;


namespace PM.Application.Commands;


public class FetchDailyPricesCommand
{
    private readonly IEnumerable<IPriceProvider> _providers;
    private readonly IPriceRepository _repository;
    private readonly IMarketCalendar _calendar;
    private readonly List<Symbol> _symbols;

    public FetchDailyPricesCommand(
        IEnumerable<IPriceProvider> providers,
        IPriceRepository repository,
        IMarketCalendar calendar,
        List<Symbol> symbols
        )
    {
        _providers = providers;
        _repository = repository;
        _calendar = calendar;
        _symbols = symbols;
    }

    /// <summary>
    /// Fetch prices for provided symbols for the given date. If allowMarketClosed = true then
    /// the command will still run for closed markets (useful for on-demand historical fetches).
    /// </summary>
    public async Task<FetchPricesDTO> ExecuteAsync(DateOnly date, bool allowMarketClosed = false)
    {
        var fetched = new List<string>();
        var skipped = new List<string>();
        var errors = new List<string>();

        foreach (var symbol in _symbols)
        {
            var exchange = symbol.Exchange;
            var marketOpen = _calendar.IsMarketOpen(date, exchange);

            // Skip if market closed and not explicitly allowed
            if (!marketOpen && !allowMarketClosed)
            {
                skipped.Add($"{symbol.Value} ({exchange}) - market closed");
                continue;
            }

            // If today, only after close
            if (date == DateOnly.FromDateTime(DateTime.Today) && !_calendar.IsAfterMarketClose(exchange))
            {
                skipped.Add($"{symbol.Value} ({exchange}) - before market close");
                continue;
            }

            // Skip if already exists
            var existing = await _repository.GetAsync(symbol, date);
            if (existing != null)
            {
                skipped.Add($"{symbol.Value} ({exchange}) - already in DB");
                continue;
            }

            // Provider selection
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
                var price = await provider.GetPriceAsync(symbol, date);
                if (price != null)
                {
                    var toSave = price with { Source = provider.ProviderName };
                    await _repository.SaveAsync(toSave);
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

        return new FetchPricesDTO(date, fetched, skipped, errors);
    }

}