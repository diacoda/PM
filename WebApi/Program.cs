using PM.Application.Interfaces;
using PM.Infrastructure.Providers;
using PM.Application.Services;
using PM.Infrastructure.Repositories;
using PM.Infrastructure.Services;
using model.Services;
using Microsoft.AspNetCore.Builder;
using PM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabase(builder.Configuration, builder.Environment);
builder.Services.AddHttpClient();

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

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IHoldingService, HoldingService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IValuationService, ValuationService>();
builder.Services.AddScoped<ITradeCostService, TradeCostService>();
builder.Services.AddScoped<ICashFlowService, CashFlowService>();
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

