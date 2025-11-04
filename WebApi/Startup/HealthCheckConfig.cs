using HealthChecks.UI.Client;
using PM.Infrastructure.Data;
using PM.Infrastructure.Health;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace PM.API.Startup
{
    /// <summary>
    /// Provides configuration methods for registering and mapping health checks.
    /// </summary>
    /// <remarks>
    /// This class configures both liveness and readiness probes for the application,
    /// allowing for better observability and integration with container orchestrators (e.g., Kubernetes).
    /// </remarks>
    public static class HealthCheckConfig
    {
        /// <summary>
        /// Registers application health checks and their dependencies in the service container.
        /// </summary>
        /// <param name="services">The service collection to add the health checks to.</param>
        /// <param name="config">The application configuration (not used currently but reserved for extension).</param>
        /// <returns>The updated <see cref="IServiceCollection"/> instance for chaining.</returns>
        /// <remarks>
        /// Adds:
        /// <list type="bullet">
        /// <item><description><see cref="PortfolioDbContext"/> health check to verify database connectivity.</description></item>
        /// <item><description><see cref="PriceProviderHealthCheck"/> to verify external price data provider availability.</description></item>
        /// </list>
        /// Health checks are tagged as <c>"ready"</c> to distinguish them from liveness probes.
        /// </remarks>
        public static IServiceCollection AddHealthChecksWithDependencies(this IServiceCollection services, IConfiguration config)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<PortfolioDbContext>("db", tags: new[] { "ready" })
                .AddCheck<PriceProviderHealthCheck>("price", tags: new[] { "ready" });

            return services;
        }

        /// <summary>
        /// Maps HTTP endpoints for the application's liveness and readiness health checks.
        /// </summary>
        /// <param name="app">The <see cref="WebApplication"/> to configure endpoints for.</param>
        /// <returns>The configured <see cref="WebApplication"/> instance.</returns>
        /// <remarks>
        /// <para>
        /// Exposes two endpoints:
        /// </para>
        /// <list type="bullet">
        /// <item><description><c>/health/live</c> — Basic liveness probe, always returns 200 if the app is running.</description></item>
        /// <item><description><c>/health/ready</c> — Readiness probe that checks dependencies (DB, price provider, etc.).</description></item>
        /// </list>
        /// <para>
        /// The readiness endpoint uses <see cref="UIResponseWriter.WriteHealthCheckUIResponse"/> for
        /// a JSON response compatible with <c>HealthChecks.UI</c>.
        /// </para>
        /// </remarks>
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
}
