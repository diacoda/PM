using PM.API.HostedServices;
using PM.Application.Commands;
using PM.Application.Interfaces;
using PM.Application.Services;
using PM.Infrastructure.Configuration;

namespace PM.API.Startup
{
    /// <summary>
    /// Provides extension methods to register background hosted jobs and related supporting services.
    /// </summary>
    /// <remarks>
    /// This class centralizes registration of recurring background jobs (e.g., fetching daily prices)
    /// and the services they depend on, such as <see cref="IMarketCalendar"/>.
    /// 
    /// Hosted services and their dependencies are registered with the DI container here.
    /// </remarks>
    public static class HostedServiceConfig
    {
        /// <summary>
        /// Registers hosted jobs and supporting services in the dependency injection container.
        /// </summary>
        /// <param name="services">The DI service collection.</param>
        /// <param name="config">The application configuration containing relevant sections.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
        /// <example>
        /// Usage in <c>Program.cs</c>:
        /// <code>
        /// builder.Services.AddHostedJobs(builder.Configuration);
        /// </code>
        /// </example>
        public static IServiceCollection AddHostedJobs(this IServiceCollection services, IConfiguration config)
        {

            // Bind market holidays from configuration and register a singleton calendar
            var holidays = config.GetSection("MarketHolidays").Get<MarketHolidaysConfig>() ?? new MarketHolidaysConfig();
            services.AddSingleton<IMarketCalendar>(new MarketCalendar(holidays));
            services.AddScoped<IDailyPriceAggregator, DailyPriceAggregator>();
            // Register the hosted background job for daily price updates
            services.AddHostedService<DailyPriceService>();

            // Register the command that supports the hosted service
            services.AddScoped<FetchDailyPricesCommand>();

            services.AddScoped<IValuationScheduler, ValuationScheduler>();
            services.AddHostedService<DailyValuationService>();

            return services;
        }
    }
}
