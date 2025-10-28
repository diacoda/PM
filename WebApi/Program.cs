using PM.Application.Interfaces;
using PM.Infrastructure.Providers;
using PM.Application.Services;
using PM.Infrastructure.Repositories;
using PM.Infrastructure.Services;
using PM.Domain.Values;
using PM.Application.Commands;
using PM.API.HostedServices;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabase(builder.Configuration, builder.Environment);
builder.Services.AddHttpClient();

// Symbols
var symbols = builder.Configuration
    .GetSection("Symbols")
    .Get<List<SymbolConfig>>()!
    .Select(s => new Symbol(s.Value, s.Currency, s.Exchange))
    .ToList();
builder.Services.AddSingleton(symbols); // List<Symbol> as singleton for DI

// Market Calendar / Holidays
var holidays = builder.Configuration
    .GetSection("MarketHolidays")
    .GetChildren()
    .ToDictionary(
        x => x.Key,
        x => x.Get<List<string>>()!.Select(DateOnly.Parse).ToList()
    );
builder.Services.AddSingleton<IMarketCalendar>(new MarketCalendar(holidays));

// Hosted Services
builder.Services.AddHostedService<DailyPriceService>();

// Commands / Providers / Repositories
builder.Services.AddScoped<FetchDailyPricesCommand>();

builder.Services.AddSingleton<IFxRateProvider, DynamicFxRateProvider>();
builder.Services.AddSingleton<IPriceProvider, InvestingPriceProvider>();
builder.Services.AddSingleton<IPriceProvider, YahooPriceProvider>();
builder.Services.AddScoped<IPricingService, PricingService>();

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IHoldingRepository, HoldingRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<IValuationRepository, ValuationRepository>();
builder.Services.AddScoped<ICashFlowRepository, CashFlowRepository>();
builder.Services.AddScoped<IPriceRepository, PriceRepository>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IHoldingService, HoldingService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IValuationService, ValuationService>();
builder.Services.AddScoped<ITradeCostService, TradeCostService>();
builder.Services.AddScoped<ICashFlowService, CashFlowService>();
builder.Services.AddScoped<IPriceService, PriceService>();
builder.Services.AddScoped<IAccountManager, AccountManager>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerUI();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbPaths = scope.ServiceProvider.GetRequiredService<DatabasePaths>();
    logger.LogInformation("Portfolio DB: {path}", dbPaths.PortfolioPath);
    logger.LogInformation("CashFlow DB : {path}", dbPaths.CashFlowPath);
    logger.LogInformation("Valuation DB: {path}", dbPaths.ValuationPath);
}

app.Run();

