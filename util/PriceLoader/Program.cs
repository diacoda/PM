using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PM.Application.Interfaces;
using PM.Application.Services;
using PM.Domain.Values;
using PM.Infrastructure.Configuration;
using PM.Infrastructure.Data;
using PM.Infrastructure.Providers;
using PM.Infrastructure.Repositories;
using PM.Infrastructure.Services;
using SQLitePCL;

IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

// Setup DI container
var services = new ServiceCollection();
services.AddMemoryCache();
services.AddHttpClient();

// Bind configuration sections
services.Configure<MarketHolidaysConfig>(config.GetSection("MarketHolidays"));
services.Configure<List<SymbolConfig>>(config.GetSection("Symbols"));

// Register dependencies required by your pricing system
var symbolConfigs = config.GetSection("Symbols").Get<List<SymbolConfig>>() ?? new List<SymbolConfig>();
var symbols = symbolConfigs
    .Select(s => new Symbol(s.Value))
    .ToList();

services.AddSingleton(symbols); // List<Symbol> as singleton for DI
services.AddSingleton<IEnumerable<Symbol>>(symbols); // IEnumerable<Symbol>

var valuationPath = DatabasePathResolver.ResolveAbsolutePath("valuation", config);
services.AddDbContext<ValuationDbContext>(options =>
    options.UseSqlite(DatabasePathResolver.BuildSqliteConnectionString(valuationPath)));

services.AddSingleton<IPriceProvider, InvestingPriceProvider>();
services.AddSingleton<IPriceProvider, YahooPriceProvider>();
services.AddSingleton<IFxRateProvider, YahooFxProvider>();

// Scoped pricing service that uses the providers
services.AddSingleton<IPricingService, PricingService>();
services.AddSingleton<IPriceRepository, PriceRepository>();
services.AddSingleton<IFxRateRepository, FxRateRepository>();
services.AddSingleton<IFxRateService, FxRateService>();
services.AddSingleton<IPriceService, PriceService>();

var holidays = config.GetSection("MarketHolidays").Get<MarketHolidaysConfig>() ?? new MarketHolidaysConfig();
services.AddSingleton<IMarketCalendar>(new MarketCalendar(holidays));

var provider = services.BuildServiceProvider();

var priceService = provider.GetRequiredService<IPriceService>();
var rateService = provider.GetRequiredService<IFxRateService>();
var calendar = provider.GetRequiredService<IMarketCalendar>();



var importer = new SimpleCsvPriceImporter(priceService, calendar);
await importer.ImportAsync(config["PricesPath"]);


var currencyPairs = new (string Base, string Quote)[]
{
    ("USD", "CAD"),
    ("USD", "EUR"),
    ("EUR", "CAD"),
    ("CAD", "USD"),
    ("EUR", "USD"),
    ("CAD", "EUR"),
};

// Start at first valid market day
DateOnly start = new DateOnly(2025, 1, 1);
DateOnly end = new DateOnly(2025, 11, 16);

var current = calendar.IsMarketOpen(start)
    ? start
    : calendar.GetNextMarketDay(start);

/*
while (current <= end)
{
    Console.WriteLine($"\n=== {current} ===");

    // --- FX RATES ---
    foreach (var pair in currencyPairs)
    {
        await ExecuteSafeAsync(
            () => rateService.GetOrFetchRateAsync(pair.Base, pair.Quote, current),
            $"FX {pair.Base}/{pair.Quote}"
        );
    }

    // --- EQUITY PRICES ---
    foreach (var symbol in symbols)
    {
        if (!symbol.Code.EndsWith(".TO", StringComparison.OrdinalIgnoreCase))
            continue;

        await ExecuteSafeAsync(
            () => priceService.GetOrFetchInstrumentPriceAsync(symbol.Code, current),
            $"Price {symbol.Code}"
        );
    }

    current = calendar.GetNextMarketDay(current);
}

static async Task ExecuteSafeAsync(Func<Task> action, string label)
{
    try
    {
        await action();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{label}] ERROR: {ex.Message}");
    }
}
*/