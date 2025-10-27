using PM.Infrastructure.Configuration;
using PM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PM.API.Configuration;

public static class DatabaseConfig
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        // Resolve all DB paths at startup using the same resolver (deterministic).
        var portfolioPath = DatabasePathResolver.ResolveAbsolutePath("portfolio", config);
        var cashFlowPath = DatabasePathResolver.ResolveAbsolutePath("cashFlow", config);
        var valuationPath = DatabasePathResolver.ResolveAbsolutePath("valuation", config);

        // Register and log connection strings
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

        // Optional: register a startup action to log actual DB files used (useful for troubleshooting)
        //services.AddSingleton(new DatabasePaths(portfolioPath, cashFlowPath, valuationPath));

        return services;
    }
}


