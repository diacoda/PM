using PM.Infrastructure.Configuration;
using PM.API.HostedServices;

namespace PM.API.Startup
{
    /// <summary>
    /// Extension methods for configuring application-wide settings from <see cref="IConfiguration"/>.
    /// </summary>
    /// <remarks>
    /// This class centralizes configuration binding for strongly-typed options used throughout the application,
    /// including market holidays, price job options, and symbol definitions.
    /// </remarks>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Binds configuration sections to strongly-typed options and registers them with the DI container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add configuration to.</param>
        /// <param name="config">The application configuration containing the relevant sections.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
        /// <example>
        /// Usage in <c>Program.cs</c>:
        /// <code>
        /// builder.Services.AddAppConfiguration(builder.Configuration);
        /// </code>
        /// </example>
        public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration config)
        {
            // Bind market holidays from configuration
            services.Configure<MarketHolidaysConfig>(config.GetSection("MarketHolidays"));

            // Bind options for scheduled price jobs
            services.Configure<PriceJobOptions>(config.GetSection("PriceJobOptions"));

            // Bind list of symbols used in the application
            services.Configure<List<SymbolConfig>>(config.GetSection("Symbols"));

            return services;
        }
    }
}
