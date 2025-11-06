using PM.API.Middleware;

namespace PM.API.Startup;

/// <summary>
/// Provides extension methods for registering custom middleware components
/// used in the Portfolio Manager API pipeline.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds the <see cref="RequestContextLoggingMiddleware"/> to the application's
    /// request processing pipeline.
    /// </summary>
    /// <param name="app">The application builder used to configure the HTTP request pipeline.</param>
    /// <returns>
    /// The same <see cref="IApplicationBuilder"/> instance so that additional configuration
    /// calls can be chained.
    /// </returns>
    /// <remarks>
    /// This middleware logs contextual information about each incoming HTTP request,
    /// such as request path, correlation ID, and execution duration, for diagnostic
    /// and monitoring purposes.
    /// </remarks>
    public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();
        return app;
    }
}
