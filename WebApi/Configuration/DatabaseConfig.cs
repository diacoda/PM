using PM.Infrastructure.Configuration;
using PM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PM.API.Configuration;

public static class DatabaseConfig
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var dbAbsolutePath = DatabasePathResolver.ResolveAbsolutePath(config);
        var connString = DatabasePathResolver.BuildSqliteConnectionString(dbAbsolutePath);

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlite(connString);
        }, ServiceLifetime.Scoped);

        return services;
    }
}
