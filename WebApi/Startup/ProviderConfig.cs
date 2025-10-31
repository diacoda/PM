using PM.Infrastructure.Providers;
using PM.Application.Interfaces;
using PM.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace PM.API.Startup
{
    /// <summary>
    /// Provides extension methods to register external data providers and related services.
    /// </summary>
    /// <remarks>
    /// This class centralizes registration of singleton and scoped services for market data,
    /// pricing, and FX rate providers. All providers are injected via DI to allow easy testing
    /// and replacement.
    /// </remarks>
    public static class ProviderConfig
    {
        /// <summary>
        /// Registers external providers and related services into the dependency injection container.
        /// </summary>
        /// <param name="services">The DI service collection.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
        /// <example>
        /// Usage in <c>Program.cs</c>:
        /// <code>
        /// builder.Services.AddProviders();
        /// </code>
        /// </example>
        public static IServiceCollection AddProviders(this IServiceCollection services)
        {
            // FX rate provider
            services.AddSingleton<IFxRateProvider, YahooFxProvider>();

            // Price providers (multiple implementations)
            services.AddSingleton<IPriceProvider, InvestingPriceProvider>();
            services.AddSingleton<IPriceProvider, YahooPriceProvider>();

            // Scoped pricing service that uses the providers
            services.AddScoped<IPricingService, PricingService>();

            return services;
        }
    }
}
