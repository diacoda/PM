using PM.Infrastructure.Configuration;
using PM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PM.API.Configuration;

public static class DatabaseConfig
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var dbAbsolutePath = DatabasePathResolver.ResolveAbsolutePath("portfolio", config);
        var connString = DatabasePathResolver.BuildSqliteConnectionString(dbAbsolutePath);

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlite(connString);
        }, ServiceLifetime.Scoped);

        dbAbsolutePath = DatabasePathResolver.ResolveAbsolutePath("cashFlow", config);
        connString = DatabasePathResolver.BuildSqliteConnectionString(dbAbsolutePath);
        services.AddDbContext<CashFlowDbContext>((sp, options) =>
        {
            options.UseSqlite(connString);
        }, ServiceLifetime.Scoped);

        dbAbsolutePath = DatabasePathResolver.ResolveAbsolutePath("valuation", config);
        connString = DatabasePathResolver.BuildSqliteConnectionString(dbAbsolutePath);
        services.AddDbContext<ValuationDbContext>((sp, options) =>
        {
            options.UseSqlite(connString);
        }, ServiceLifetime.Scoped);

        return services;
    }
}
