using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace PM.API.Configuration
{
    /// <summary>
    /// Provides centralized configuration for Serilog-based application logging.
    /// </summary>
    /// <remarks>
    /// This class configures Serilog to:
    /// <list type="bullet">
    ///   <item>Read settings from the application's configuration file.</item>
    ///   <item>Enrich logs with environment, process, thread, and trace context.</item>
    ///   <item>Write logs to both the console and JSON files under <c>../logs/app.log</c>.</item>
    /// </list>
    /// Example <c>appsettings.json</c> section:
    /// <code>
    /// "Serilog": {
    ///   "MinimumLevel": "Information",
    ///   "WriteTo": [
    ///     { "Name": "Console" },
    ///     { "Name": "File", "Args": { "path": "../logs/app.log" } }
    ///   ]
    /// }
    /// </code>
    /// </remarks>
    public static class LoggingConfig
    {
        /// <summary>
        /// Configures the application to use Serilog for structured logging.
        /// </summary>
        /// <param name="host">The <see cref="IHostBuilder"/> instance being configured.</param>
        /// <param name="config">The application's configuration source.</param>
        /// <returns>The same <see cref="IHostBuilder"/> for chaining.</returns>
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
                   .WriteTo.File(
                       formatter: new JsonFormatter(),
                       path: "../logs/app.log",
                       rollingInterval: RollingInterval.Day,
                       retainedFileCountLimit: 7
                   );
            });

            return host;
        }
    }
}
