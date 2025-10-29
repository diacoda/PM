using PM.Infrastructure.Configuration;
using PM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PM.API.Configuration;

/// <summary>
/// Provides extension methods to configure database contexts for the API.
/// </summary>
public static class DatabaseConfig
{
    /// <summary>
    /// Adds EF Core DbContexts for portfolio, cash flow, and valuation databases to the DI container.
    /// Resolves SQLite paths using <see cref="DatabasePathResolver"/> and registers them as scoped services.
    /// Also registers <see cref="DatabasePaths"/> as a singleton for optional diagnostics/logging.
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <param name="config">The application configuration for resolving relative paths.</param>
    /// <param name="env">The hosting environment (optional, for logging or environment-specific behavior).</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        // Resolve all database paths at startup deterministically
        var portfolioPath = DatabasePathResolver.ResolveAbsolutePath("portfolio", config);
        var cashFlowPath = DatabasePathResolver.ResolveAbsolutePath("cashFlow", config);
        var valuationPath = DatabasePathResolver.ResolveAbsolutePath("valuation", config);

        // Configure DbContexts with resolved SQLite connection strings
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var conn = DatabasePathResolver.BuildSqliteConnectionString(portfolioPath);
            options.UseSqlite(conn);
        }, ServiceLifetime.Scoped);

        services.AddDbContext<CashFlowDbContext>((sp, options) =>
        {
            var conn = DatabasePathResolver.BuildSqliteConnectionString(cashFlowPath);
            options.UseSqlite(conn);
        }, ServiceLifetime.Scoped);

        services.AddDbContext<ValuationDbContext>((sp, options) =>
        {
            var conn = DatabasePathResolver.BuildSqliteConnectionString(valuationPath);
            options.UseSqlite(conn);
        }, ServiceLifetime.Scoped);

        // Register DatabasePaths singleton for optional diagnostics/logging
        services.AddSingleton(new DatabasePaths(portfolioPath, cashFlowPath, valuationPath));

        return services;
    }
}
