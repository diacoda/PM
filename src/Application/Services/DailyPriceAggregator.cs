namespace PM.Application.Services;

// DailyPriceAggregator.cs
using System.Linq;
using PM.Application.Commands;
using PM.Application.Interfaces;
using PM.Domain.Values;
using PM.DTO.Prices;

public class DailyPriceAggregator : IDailyPriceAggregator
{
    private readonly FetchDailyPricesCommand _fetchCommand;
    private readonly IMarketCalendar _calendar;
    private readonly IEnumerable<Symbol> _symbols;

    public DailyPriceAggregator(
        FetchDailyPricesCommand fetchCommand,
        IMarketCalendar calendar,
        IEnumerable<Symbol> symbols)
    {
        _fetchCommand = fetchCommand;
        _calendar = calendar;
        _symbols = symbols;
    }

    public async Task<FetchPricesDTO> RunOnceAsync(DateOnly date, bool allowMarketClosed = false, CancellationToken ct = default)
    {
        // Strategy:
        // - group by exchange so the fetcher/command can skip closed markets or allow closed if flagged
        // - delegate to command which will return per-symbol details (fetched/skipped/errors)
        return await _fetchCommand.ExecuteAsync(date, allowMarketClosed, ct);
    }
}
