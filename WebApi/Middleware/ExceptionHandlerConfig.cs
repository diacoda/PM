using Microsoft.AspNetCore.Diagnostics;

namespace PM.API.Middleware
{
    /// <summary>
    /// Provides an extension method to configure a global exception handler
    /// that returns structured problem details with correlation ID and domain error info.
    /// </summary>
    public static class ExceptionHandlerConfig
    {
        /// <summary>
        /// Registers a global exception handler in the ASP.NET Core middleware pipeline.
        /// </summary>
        /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
        /// <returns>The same <see cref="WebApplication"/> instance, for chaining.</returns>
        /// <remarks>
        /// This middleware catches all unhandled exceptions and delegates to
        /// <see cref="ProblemDetailsWriter"/> to generate a structured, RFC 7807-compliant
        /// response that includes a correlation ID (from <c>Correlation-Id</c> header or
        /// <see cref="HttpContext.TraceIdentifier"/>).
        /// </remarks>
        public static WebApplication UseGlobalExceptionHandler(this WebApplication app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("GlobalException");

                    var feature = context.Features.Get<IExceptionHandlerFeature>();
                    var ex = feature?.Error ?? new Exception("Unknown error");

                    await ProblemDetailsWriter.WriteAsync(context, ex, logger);
                });
            });

            return app;
        }
    }
}
