var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabase(builder.Configuration, builder.Environment);

//builder.Services.AddSingleton<IFxRateProvider, DynamicFxRateProvider>();
//builder.Services.AddSingleton<IPriceProvider, DynamicPriceProvider>();

builder.Services.AddScoped<PM.Application.Interfaces.IAccountRepository,
    PM.Infrastructure.Repositories.AccountRepository>();
builder.Services.AddScoped<PM.Application.Interfaces.IHoldingRepository,
    PM.Infrastructure.Repositories.HoldingRepository>();
builder.Services.AddScoped<PM.Application.Interfaces.ITransactionRepository,
    PM.Infrastructure.Repositories.TransactionRepository>();
builder.Services.AddScoped<PM.Application.Interfaces.IPortfolioRepository,
    PM.Infrastructure.Repositories.PortfolioRepository>();

builder.Services.AddOpenApi();
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();
app.Run();

