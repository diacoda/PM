using Microsoft.Extensions.DependencyInjection;
using PM.Application.Interfaces;
using PM.Infrastructure.Repositories;

namespace PM.API.Startup
{
    /// <summary>
    /// Provides extension methods to register repository implementations for dependency injection.
    /// </summary>
    /// <remarks>
    /// This class centralizes registration of all repositories (Account, Holding, Transaction, Portfolio, etc.)
    /// as scoped services to ensure one instance per request or scope.
    /// </remarks>
    public static class RepositoryConfig
    {
        /// <summary>
        /// Registers repository implementations into the DI container.
        /// </summary>
        /// <param name="services">The DI service collection.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
        /// <example>
        /// Usage in <c>Program.cs</c>:
        /// <code>
        /// builder.Services.AddRepositories();
        /// </code>
        /// </example>
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
}
