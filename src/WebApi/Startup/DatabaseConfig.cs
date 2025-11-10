using Microsoft.EntityFrameworkCore;
using PM.Infrastructure.Configuration;
using PM.Infrastructure.Data;

namespace PM.API.Startup;

/// <summary>
/// Provides extension methods to configure EF Core database contexts for the API.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Adds and configures EF Core database contexts for portfolio, cash flow, and valuation databases.
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <param name="config">The app configuration, used to resolve relative paths.</param>
    /// <param name="env">The hosting environment (optional, for environment-specific behavior).</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        if (env.IsEnvironment("IntegrationTests"))
            return services.AddInMemorySqliteDatabases();

        // Regular file-based SQLite for Development/Production
        var portfolioPath = DatabasePathResolver.ResolveAbsolutePath("portfolio", config, env);
        var cashFlowPath = DatabasePathResolver.ResolveAbsolutePath("cashFlow", config, env);
        var valuationPath = DatabasePathResolver.ResolveAbsolutePath("valuation", config, env);

        services.AddDbContext<PortfolioDbContext>(options =>
            options.UseSqlite(DatabasePathResolver.BuildSqliteConnectionString(portfolioPath)));

        services.AddDbContext<CashFlowDbContext>(options =>
            options.UseSqlite(DatabasePathResolver.BuildSqliteConnectionString(cashFlowPath)));

        services.AddDbContext<ValuationDbContext>(options =>
            options.UseSqlite(DatabasePathResolver.BuildSqliteConnectionString(valuationPath)));

        services.AddSingleton(new DatabasePaths(portfolioPath, cashFlowPath, valuationPath));
        return services;
    }
}
