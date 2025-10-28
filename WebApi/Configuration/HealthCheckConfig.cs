using HealthChecks.UI.Client;
using PM.Infrastructure.Data;
using PM.Infrastructure.Health;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace PM.API.Configuration;

public static class HealthCheckConfig
{
    public static IServiceCollection AddHealthChecksWithDependencies(this IServiceCollection services, IConfiguration config)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("db", tags: new[] { "ready" })
            .AddCheck<PriceProviderHealthCheck>("price", tags: new[] { "ready" });
        return services;
    }

    public static WebApplication MapHealthChecksWithUI(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}
