using PM.DTO.Prices;
using PM.Application.Interfaces;
using PM.Domain.Values;
using Microsoft.Extensions.Caching.Memory;

namespace PM.Application.Services;

public class PriceService : IPriceService
{
    private readonly IPriceRepository _repository;
    private readonly List<Symbol> _symbols;
    private readonly IEnumerable<IPriceProvider> _providers;
    private readonly IMemoryCache _cache;

    // Default cache duration (adjust as needed)
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(12);

    public PriceService(
        IPriceRepository repository,
        List<Symbol> symbols,
        IEnumerable<IPriceProvider> providers,
        IMemoryCache cache)
    {
        _repository = repository;
        _symbols = symbols;
        _providers = providers;
        _cache = cache;
    }

    public async Task<InstrumentPrice?> GetOrFetchInstrumentPriceAsync(
        string symbolValue,
        DateOnly date,
        CancellationToken ct = default)
    {
        var symbol = _symbols.FirstOrDefault(s =>
            s.Value.Equals(symbolValue, StringComparison.OrdinalIgnoreCase));

        if (symbol is null)
            throw new ArgumentException($"Symbol '{symbolValue}' is not in the accepted symbols list.");

        var dbPrice = await _repository.GetAsync(symbol, date, ct);
        if (dbPrice is not null)
        {
            return dbPrice;
        }

        string providerName = symbol.Value.EndsWith(".TO", StringComparison.OrdinalIgnoreCase)
            ? "Yahoo"
            : "Memory";

        var provider = _providers.SingleOrDefault(p =>
            p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        if (provider is null)
            throw new InvalidOperationException($"No provider found for {symbolValue} ({symbol.Exchange}).");

        var fetched = await provider.GetPriceAsync(symbol, date, ct);
        if (fetched is null || fetched.Price.Amount <= 0)
            throw new InvalidOperationException($"No valid price found for {symbolValue} on {date}.");

        await _repository.UpsertAsync(fetched, ct);
        return fetched;
    }

    public async Task<PriceDTO?> GetPriceAsync(string symbolValue, DateOnly date, CancellationToken ct = default)
    {
        var symbol = _symbols.FirstOrDefault(s => s.Value.Equals(symbolValue, StringComparison.OrdinalIgnoreCase));
        if (symbol is null)
            throw new ArgumentException($"Symbol '{symbolValue}' is not in the accepted symbols list.");

        string cacheKey = BuildCacheKey(symbolValue, date);

        // 1️⃣ Try cache
        if (_cache.TryGetValue(cacheKey, out PriceDTO? cached))
            return cached;

        // 2️⃣ Try repository
        var dbPrice = await _repository.GetAsync(symbol, date, ct);
        if (dbPrice is not null)
        {
            var dto = new PriceDTO
            {
                Symbol = dbPrice.Symbol.Value,
                Date = dbPrice.Date,
                Close = dbPrice.Price.Amount
            };

            CachePrice(cacheKey, dto);
            return dto;
        }

        // 3️⃣ Try provider
        string providerName = symbol.Value.EndsWith(".TO", StringComparison.OrdinalIgnoreCase)
            ? "Yahoo"
            : "Memory";

        var provider = _providers.SingleOrDefault(p =>
            p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        if (provider is null)
            throw new InvalidOperationException($"No provider found for {symbolValue} ({symbol.Exchange}).");

        var fetched = await provider.GetPriceAsync(symbol, date, ct);
        if (fetched is null || fetched.Price.Amount <= 0)
            throw new InvalidOperationException($"No valid price found for {symbolValue} on {date}.");

        await _repository.UpsertAsync(fetched, ct);

        var dtoFromProvider = new PriceDTO
        {
            Symbol = fetched.Symbol.Value,
            Date = fetched.Date,
            Close = fetched.Price.Amount
        };

        CachePrice(cacheKey, dtoFromProvider);
        return dtoFromProvider;
    }

    public async Task<PriceDTO> UpdatePriceAsync(string symbolValue, UpdatePriceRequest request, CancellationToken ct)
    {
        var symbol = _symbols.FirstOrDefault(s => s.Value.Equals(symbolValue, StringComparison.OrdinalIgnoreCase));
        if (symbol is null)
            throw new ArgumentException($"Symbol '{symbolValue}' is not in the accepted symbols list.");

        var currency = new Currency(symbol.Currency);
        var money = new Money(request.Close, currency);
        var price = new InstrumentPrice(symbol, request.Date, money, currency, "Manual Entry");

        await _repository.UpsertAsync(price, ct);

        var dto = new PriceDTO
        {
            Symbol = price.Symbol.Value,
            Date = price.Date,
            Close = price.Price.Amount
        };

        CachePrice(BuildCacheKey(symbol.Value, request.Date), dto);
        return dto;
    }

    public async Task<List<PriceDTO>> GetAllPricesForSymbolAsync(string symbolValue, CancellationToken ct)
    {
        var symbol = _symbols.FirstOrDefault(s => s.Value.Equals(symbolValue, StringComparison.OrdinalIgnoreCase));
        if (symbol is null)
            throw new ArgumentException($"Symbol '{symbolValue}' is not in the accepted symbols list.");

        var prices = await _repository.GetAllForSymbolAsync(symbol, ct);
        return prices.Select(p => new PriceDTO
        {
            Symbol = p.Symbol.Value,
            Date = p.Date,
            Close = p.Price.Amount
        }).ToList();
    }

    public async Task<bool> DeletePriceAsync(string symbolValue, DateOnly date, CancellationToken ct)
    {
        var symbol = _symbols.FirstOrDefault(s => s.Value.Equals(symbolValue, StringComparison.OrdinalIgnoreCase));
        if (symbol is null)
            throw new ArgumentException($"Symbol '{symbolValue}' is not in the accepted symbols list.");

        string cacheKey = BuildCacheKey(symbol.Value, date);
        _cache.Remove(cacheKey);

        return await _repository.DeleteAsync(symbol, date, ct);
    }

    public async Task<List<PriceDTO>> GetAllPricesByDateAsync(DateOnly date, CancellationToken ct)
    {
        var prices = await _repository.GetAllByDateAsync(date, ct);
        var map = prices.ToDictionary(p => p.Symbol.Value, StringComparer.OrdinalIgnoreCase);

        return _symbols.Select(s =>
        {
            if (map.TryGetValue(s.Value, out var p))
            {
                var dto = new PriceDTO
                {
                    Symbol = p.Symbol.Value,
                    Date = p.Date,
                    Close = p.Price.Amount
                };

                CachePrice(BuildCacheKey(s.Value, date), dto);
                return dto;
            }

            return new PriceDTO
            {
                Symbol = s.Value,
                Date = date,
                Close = 0
            };
        }).ToList();
    }

    private static string BuildCacheKey(string symbol, DateOnly date)
        => $"{symbol.ToUpperInvariant()}:{date:yyyyMMdd}";

    private void CachePrice(string key, PriceDTO dto)
    {
        _cache.Set(key, dto, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheDuration
        });
    }
}
