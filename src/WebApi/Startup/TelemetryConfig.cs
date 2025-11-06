using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace PM.API.Startup;

/// <summary>
/// Provides extension methods to configure OpenTelemetry tracing, metrics, and logging
/// for the Portfolio Management API.
/// </summary>
/// <remarks>
/// This setup includes:
/// <list type="bullet">
/// <item><description>ASP.NET Core, HTTP client, and EF Core instrumentation.</description></item>
/// <item><description>Console and OTLP (OpenTelemetry Protocol) exporters.</description></item>
/// <item><description>Prometheus metrics endpoint.</description></item>
/// <item><description>Exclusion of /health, /metrics, and /telemetry endpoints from tracing.</description></item>
/// </list>
/// </remarks>
public static class TelemetryConfig
{
    /// <summary>
    /// Adds OpenTelemetry tracing and metrics configuration to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add telemetry to.</param>
    /// <param name="config">The application configuration.</param>
    /// <param name="env">The hosting environment, used to adjust telemetry behavior for development or production.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with telemetry services registered.</returns>
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        const string serviceName = "PortfolioManagement.API";
        const string serviceVersion = "1.0.0";

        // Register OpenTelemetry
        services.AddOpenTelemetry()
            .ConfigureResource(r =>
                r.AddService(serviceName: serviceName, serviceVersion: serviceVersion))
            .WithTracing(t => t
                .AddSource(serviceName, serviceVersion)
                .AddAspNetCoreInstrumentation(o =>
                {
                    o.RecordException = true;
                    // Exclude noise from telemetry
                    o.Filter = ctx =>
                    {
                        var path = ctx.Request.Path;
                        return !(
                            path.StartsWithSegments("/health") ||
                            path.StartsWithSegments("/metrics") ||
                            path.StartsWithSegments("/telemetry")
                        );
                    };
                })
                .AddHttpClientInstrumentation(o => o.RecordException = true)
                .AddEntityFrameworkCoreInstrumentation()
                .AddConsoleExporter()
                .AddOtlpExporter())
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddConsoleExporter()
                .AddMeter(serviceName, serviceVersion)
                .AddOtlpExporter()
                .AddPrometheusExporter());

        // Register OpenTelemetry sources for manual tracing/metrics
        var activitySource = new ActivitySource(serviceName, serviceVersion);
        var meter = new Meter(serviceName, serviceVersion);

        services.AddSingleton(activitySource);
        services.AddSingleton(meter);

        return services;
    }
}
