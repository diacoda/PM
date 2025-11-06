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
        // Resolve all absolute database file paths
        var portfolioPath = DatabasePathResolver.ResolveAbsolutePath("portfolio", config);
        var cashFlowPath = DatabasePathResolver.ResolveAbsolutePath("cashFlow", config);
        var valuationPath = DatabasePathResolver.ResolveAbsolutePath("valuation", config);

        // Register each DbContext using scoped lifetime
        services.AddDbContext<PortfolioDbContext>(options =>
            options.UseSqlite(DatabasePathResolver.BuildSqliteConnectionString(portfolioPath)));

        services.AddDbContext<CashFlowDbContext>(options =>
            options.UseSqlite(DatabasePathResolver.BuildSqliteConnectionString(cashFlowPath)));

        services.AddDbContext<ValuationDbContext>(options =>
            options.UseSqlite(DatabasePathResolver.BuildSqliteConnectionString(valuationPath)));

        // Expose the resolved paths for diagnostics/logging
        services.AddSingleton(new DatabasePaths(portfolioPath, cashFlowPath, valuationPath));

        return services;
    }
}
