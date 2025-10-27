using PM.Application.Interfaces;
using PM.Infrastructure.Providers;
using PM.Application.Services;
using PM.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabase(builder.Configuration, builder.Environment);

builder.Services.AddSingleton<IFxRateProvider, DynamicFxRateProvider>();
builder.Services.AddSingleton<IPriceProvider, DynamicPriceProvider>();

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IHoldingRepository, HoldingRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<IValuationRepository, ValuationRepository>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IHoldingService, HoldingService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IValuationService, ValuationService>();
builder.Services.AddScoped<IAccountManager, AccountManager>();

builder.Services.AddOpenApi();
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();
app.Run();

