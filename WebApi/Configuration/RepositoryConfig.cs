using Microsoft.Extensions.DependencyInjection;
using PM.Application.Interfaces;
using PM.Infrastructure.Repositories;

namespace PM.API.Configuration;

public static class RepositoryConfig
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IHoldingRepository, HoldingRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IValuationRepository, ValuationRepository>();
        services.AddScoped<ICashFlowRepository, CashFlowRepository>();
        services.AddScoped<IPriceRepository, PriceRepository>();
        services.AddScoped<IFxRateRepository, FxRateRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        return services;
    }
}
