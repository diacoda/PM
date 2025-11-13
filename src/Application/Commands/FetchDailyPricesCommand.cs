using PM.DTO.Prices;
using PM.Application.Interfaces;
using PM.Domain.Values;
using PM.Domain.Enums;

namespace PM.Application.Commands;

/// <summary>
/// Command to fetch equity and FX daily prices, fully async-safe, sequential.
/// </summary>
public class FetchDailyPricesCommand
{
    private readonly IPriceService _priceService;
    private readonly IMarketCalendar _calendar;
    private readonly IEnumerable<Symbol> _symbols;
    private readonly IFxRateService _fxService;
    private readonly IFxRateProvider _fxProvider;
    private readonly IFxRateRepository _fxRepository;

    private static readonly Currency[] Currencies = { new("EUR"), new("USD"), new("CAD") };

    public FetchDailyPricesCommand(
        IPriceService priceService,
        IMarketCalendar calendar,
        IEnumerable<Symbol> symbols,
        IFxRateService fxService,
        IFxRateProvider fxProvider,
        IFxRateRepository fxRepository)
    {
        _priceService = priceService;
        _calendar = calendar;
        _symbols = symbols;
        _fxService = fxService;
        _fxProvider = fxProvider;
        _fxRepository = fxRepository;
    }

    public async Task<FetchPricesDTO> ExecuteAsync(DateOnly date, bool allowMarketClosed = false, CancellationToken ct = default)
    {
        var fetched = new List<string>();
        var skipped = new List<string>();
        var errors = new List<string>();
        var details = new List<SymbolFetchDetail>();

        // --- Fetch equities ---
        foreach (var symbol in _symbols)
        {
            if (ct.IsCancellationRequested) break;
            await FetchEquityAsync(symbol, date, allowMarketClosed, fetched, skipped, errors, details, ct);
        }

        // --- Fetch FX rates ---
        var fxPairs = GetFxPairs(Currencies);
        foreach (var (from, to) in fxPairs)
        {
            if (ct.IsCancellationRequested) break;
            await FetchFxAsync(from, to, date, allowMarketClosed, fetched, skipped, errors, details, ct);
        }

        // --- Map to DTO ---
        var dtoDetails = details.Select(d => new SymbolFetchDetailDTO
        {
            Symbol = d.Symbol,
            Exchange = d.Exchange,
            Status = d.Status,
            Error = d.Error,
        }).ToList();

        return new FetchPricesDTO(date, fetched, skipped, errors, dtoDetails);
    }

    #region Helpers

    private static List<(Currency from, Currency to)> GetFxPairs(Currency[] currencies)
    {
        var pairs = new List<(Currency from, Currency to)>();

        for (int i = 0; i < currencies.Length; i++)
        {
            for (int j = i + 1; j < currencies.Length; j++)
            {
                pairs.Add((currencies[i], currencies[j]));
            }
        }
        return pairs;
    }


    private async Task FetchEquityAsync(
        Symbol symbol,
        DateOnly date,
        bool allowMarketClosed,
        List<string> fetched,
        List<string> skipped,
        List<string> errors,
        List<SymbolFetchDetail> details,
        CancellationToken ct)
    {
        try
        {
            if (!IsMarketOpenForSymbol(symbol, date, allowMarketClosed))
            {
                AddDetail(skipped, details, symbol.Code, symbol.Exchange, FetchStatus.Skipped, "market closed or before market close");
                return;
            }

            var price = await _priceService.GetOrFetchInstrumentPriceAsync(symbol.Code, date, ct);
            if (price == null)
            {
                AddDetail(errors, details, symbol.Code, symbol.Exchange, FetchStatus.Error, "failed to get or fetch");
            }
            else
            {
                AddDetail(fetched, details, symbol.Code, symbol.Exchange, FetchStatus.Fetched);
            }
        }
        catch (Exception ex)
        {
            AddDetail(errors, details, symbol.Code, symbol.Exchange, FetchStatus.Error, ex.Message);
        }
    }

    private async Task FetchFxAsync(
        Currency from,
        Currency to,
        DateOnly date,
        bool allowMarketClosed,
        List<string> fetched,
        List<string> skipped,
        List<string> errors,
        List<SymbolFetchDetail> details,
        CancellationToken ct)
    {
        try
        {
            const string exchange = "FX";

            if (!IsMarketOpenForFx(date, allowMarketClosed))
            {
                AddDetail(skipped, details, $"{from.Code}/{to.Code}", exchange, FetchStatus.Skipped, "market closed or before market close");
                return;
            }

            var rate = await _fxService.GetOrFetchRateAsync(from.Code, to.Code, date, ct);
            if (rate == null)
            {
                AddDetail(errors, details, $"{from.Code}/{to.Code}", exchange, FetchStatus.Error, "failed to get or fetch");
            }
            else
            {
                AddDetail(fetched, details, $"{from.Code}/{to.Code}", exchange, FetchStatus.Fetched);
            }
        }
        catch (Exception ex)
        {
            AddDetail(errors, details, $"{from.Code}/{to.Code}", "FX", FetchStatus.Error, ex.Message);
        }
    }

    private static void AddDetail(List<string> bag, List<SymbolFetchDetail> detailsBag, string key, string exchange, FetchStatus status, string? error = null)
    {
        bag.Add(key);
        detailsBag.Add(new SymbolFetchDetail(key, exchange, status.ToString(), error));
    }

    private bool IsMarketOpenForSymbol(Symbol symbol, DateOnly date, bool allowMarketClosed)
    {
        if (!_calendar.IsMarketOpen(date, symbol.Exchange))
            return false;

        if (_calendar.IsToday(date) && !_calendar.IsAfterMarketClose(symbol.Exchange) && !allowMarketClosed)
            return false;

        return true;
    }

    private bool IsMarketOpenForFx(DateOnly date, bool allowMarketClosed)
    {
        const string exchange = "TSX"; // or FX market placeholder
        if (!_calendar.IsMarketOpen(date, exchange))
            return false;

        if (_calendar.IsToday(date) && !_calendar.IsAfterMarketClose(exchange) && !allowMarketClosed)
            return false;

        return true;
    }

    #endregion
}
