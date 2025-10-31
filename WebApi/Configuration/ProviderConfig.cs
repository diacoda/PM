using PM.Infrastructure.Providers;
using PM.Application.Interfaces;
using PM.Infrastructure.Services;

namespace PM.API.Configuration;

public static class ProviderConfig
{
    public static IServiceCollection AddProviders(this IServiceCollection services)
    {
        services.AddSingleton<IFxRateProvider, YahooFxProvider>();
        services.AddSingleton<IPriceProvider, InvestingPriceProvider>();
        services.AddSingleton<IPriceProvider, YahooPriceProvider>();
        services.AddScoped<IPricingService, PricingService>();
        return services;
    }
}
