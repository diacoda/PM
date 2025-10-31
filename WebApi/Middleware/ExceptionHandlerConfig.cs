using PM.API.Middleware;
using Microsoft.AspNetCore.Diagnostics;

namespace PM.API.Middleware
{
    /// <summary>
    /// Extension methods for configuring middleware in the WebApplication pipeline.
    /// </summary>
    public static class ExceptionHandlerConfig
    {
        /// <summary>
        /// Adds a global exception handler to catch unhandled exceptions and return a standardized
        /// problem details response.
        /// </summary>
        /// <param name="app">The <see cref="WebApplication"/> instance to configure.</param>
        /// <returns>The configured <see cref="WebApplication"/> instance.</returns>
        public static WebApplication UseGlobalExceptionHandler(this WebApplication app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var logger = context.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("GlobalException");

                    var feature = context.Features.Get<IExceptionHandlerFeature>();
                    var ex = feature?.Error ?? new Exception("Unknown error");

                    await ProblemDetailsWriter.WriteAsync(context, ex, logger);
                });
            });

            return app;
        }
    }
}
