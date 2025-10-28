using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace PM.API.Configuration;

public static class LoggingConfig
{
    public static IHostBuilder UseSerilogLogging(this IHostBuilder host, IConfiguration config)
    {
        host.UseSerilog((ctx, cfg) =>
        {
            cfg.ReadFrom.Configuration(ctx.Configuration)
               .Enrich.FromLogContext()
               .Enrich.WithEnvironmentName()
               .Enrich.WithProcessId()
               .Enrich.WithThreadId()
               .Enrich.WithSpan()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
               .WriteTo.Console()
               .WriteTo.File(new JsonFormatter(), "../logs/app.log", rollingInterval: RollingInterval.Day);
        });
        return host;
    }
}
