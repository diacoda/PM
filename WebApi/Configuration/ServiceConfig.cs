using Microsoft.Extensions.DependencyInjection;
using PM.Application.Interfaces;
using PM.Application.Services;

namespace PM.API.Configuration;

public static class ServiceConfig
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IHoldingService, HoldingService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IPortfolioService, PortfolioService>();
        services.AddScoped<IValuationService, ValuationService>();
        services.AddScoped<ITradeCostService, TradeCostService>();
        services.AddScoped<ICashFlowService, CashFlowService>();
        services.AddScoped<IPriceService, PriceService>();
        services.AddScoped<IFxRateService, FxRateService>();
        services.AddScoped<IAccountManager, AccountManager>();
        services.AddScoped<ITransactionWorkflowService, TransactionWorkflowService>();
        services.AddScoped<ITagService, TagService>();
        return services;
    }
}
