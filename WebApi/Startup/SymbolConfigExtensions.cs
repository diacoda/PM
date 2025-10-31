using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PM.Domain.Values;

namespace PM.API.Startup
{
    /// <summary>
    /// Provides extension methods to register symbol configurations in the DI container.
    /// </summary>
    public static class SymbolConfigExtensions
    {
        /// <summary>
        /// Reads the "Symbols" section from configuration, converts each <see cref="SymbolConfig"/> 
        /// to a <see cref="Symbol"/>, and registers the resulting list as a singleton.
        /// </summary>
        /// <param name="services">The DI service collection.</param>
        /// <param name="config">The application configuration.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
        /// <example>
        /// Usage in <c>Program.cs</c>:
        /// <code>
        /// builder.Services.AddSymbolConfigs(builder.Configuration);
        /// </code>
        /// </example>
        public static IServiceCollection AddSymbolConfigs(this IServiceCollection services, IConfiguration config)
        {
            var symbolConfigs = config.GetSection("Symbols").Get<List<SymbolConfig>>() ?? new List<SymbolConfig>();
            var symbols = symbolConfigs
                .Select(s => new Symbol(s.Value, s.Currency, s.Exchange))
                .ToList();

            services.AddSingleton(symbols); // List<Symbol> as singleton for DI
            return services;
        }
    }
}
