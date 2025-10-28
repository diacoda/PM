using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace PM.API.Configuration;

public static class TelemetryConfig
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var serviceName = "PortfolioManagement.API";
        var serviceVersion = "1.0.0";

        // OpenTelemetry
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName: serviceName, serviceVersion: serviceVersion))
            .WithTracing(t => t
                .AddSource(serviceName, serviceVersion)
                .AddAspNetCoreInstrumentation(o =>
                {
                    o.RecordException = true;
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

        var activitySource = new ActivitySource(serviceName, serviceVersion);
        //var activitySource = new ActivitySource(serviceName);
        var meter = new Meter(serviceName, serviceVersion);
        //var meter = new Meter(serviceName);

        services.AddSingleton(activitySource);
        services.AddSingleton(meter);

        return services;
    }


}
