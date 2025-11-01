using PM.DTO.Prices;
using PM.Application.Interfaces;
using PM.Domain.Values;

namespace PM.Application.Services;

public class PriceService : IPriceService
{
    private readonly IPriceRepository _repository;
    private readonly List<Symbol> _symbols;
    private readonly IEnumerable<IPriceProvider> _providers;

    public PriceService(
        IPriceRepository repository,
        List<Symbol> symbols,
        IEnumerable<IPriceProvider> providers
        )
    {
        _repository = repository;
        _symbols = symbols;
        _providers = providers;
    }

    public async Task<PriceDTO> UpdatePriceAsync(string symbolValue, UpdatePriceRequest request, CancellationToken ct)
    {
        var symbol = _symbols
            .FirstOrDefault(s => s.Value.Equals(symbolValue, StringComparison.OrdinalIgnoreCase));

        if (symbol is null)
            throw new ArgumentException($"Symbol '{symbolValue}' is not in the accepted symbols list.");

        Currency currency = new Currency(symbol.Currency);
        Money money = new Money(request.Close, currency);
        var price = new InstrumentPrice(
            symbol,
            request.Date,
            money,
            currency,
            "Manual Entry"
        );

        await _repository.UpsertAsync(price, ct);

        return new PriceDTO
        {
            Symbol = price.Symbol.Value,
            Date = price.Date,
            Close = price.Price.Amount
        };
    }

    public async Task<PriceDTO> FetchAndUpsertFromProviderAsync(UpsertPriceProviderRequest request, CancellationToken ct)
    {
        // Validate symbol exists
        var symbol = _symbols
            .FirstOrDefault(s => s.Value.Equals(request.Symbol, StringComparison.OrdinalIgnoreCase));

        if (symbol is null)
            throw new ArgumentException($"Symbol '{request.Symbol}' is not in the accepted symbols list.");

        // Select provider based on symbol
        string providerName = symbol.Value.EndsWith(".TO", StringComparison.OrdinalIgnoreCase)
            ? "Yahoo"
            : "Memory";

        var provider = _providers.SingleOrDefault(p =>
            p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        if (provider == null)
            throw new InvalidOperationException($"No price provider found with name '{providerName}' for {symbol.Value} ({symbol.Exchange}).");

        // Fetch price
        var fetchedPrice = await provider.GetPriceAsync(symbol, request.Date, ct);

        if (fetchedPrice is null || fetchedPrice.Price.Amount <= 0)
            throw new InvalidOperationException($"Price provider '{providerName}' did not return a valid price for {symbol.Value} on {request.Date}.");

        Currency currency = new Currency(symbol.Currency);
        Money money = new Money(fetchedPrice.Price.Amount, currency);
        // Prepare price to upsert
        var priceToUpsert = new InstrumentPrice(
            symbol,
            request.Date,
            money,
            currency,
            providerName
        );

        // Upsert the price
        await _repository.UpsertAsync(priceToUpsert, ct);

        // Return DTO
        return new PriceDTO
        {
            Symbol = priceToUpsert.Symbol.Value,
            Date = priceToUpsert.Date,
            Close = priceToUpsert.Price.Amount
        };
    }

    public async Task<PriceDTO?> GetPriceAsync(string symbolValue, DateOnly date, CancellationToken ct)
    {
        var symbol = _symbols.FirstOrDefault(s => s.Value.Equals(symbolValue, StringComparison.OrdinalIgnoreCase));
        if (symbol is null)
            throw new ArgumentException($"Symbol '{symbolValue}' is not in the accepted symbols list.");

        var price = await _repository.GetAsync(symbol, date, ct);
        if (price is null) return null;

        return new PriceDTO
        {
            Symbol = price.Symbol.Value,
            Date = price.Date,
            Close = price.Price.Amount
        };
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

        return await _repository.DeleteAsync(symbol, date, ct);
    }

    public async Task<List<PriceDTO>> GetAllPricesByDateAsync(DateOnly date, CancellationToken ct)
    {
        var prices = await _repository.GetAllByDateAsync(date, ct);

        // Build a lookup by symbol name
        var priceMap = prices.ToDictionary(p => p.Symbol.Value, StringComparer.OrdinalIgnoreCase);

        // Map symbols to DTOs
        return _symbols.Select(s =>
        {
            if (priceMap.TryGetValue(s.Value, out var p))
            {
                return new PriceDTO
                {
                    Symbol = s.Value,
                    Date = p.Date,
                    Close = p.Price.Amount
                };
            }

            return new PriceDTO
            {
                Symbol = s.Value,
                Date = date,
                Close = 0 // or null if you later switch to nullable decimal
            };
        }).ToList();
    }
}