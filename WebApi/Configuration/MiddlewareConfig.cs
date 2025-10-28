using PM.API.Middleware;
using Microsoft.AspNetCore.Diagnostics;

namespace PM.API.Configuration;

public static class MiddlewareConfig
{
    public static WebApplication UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("GlobalException");
                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var ex = feature?.Error ?? new Exception("Unknown error");
                await ProblemDetailsWriter.WriteAsync(context, ex, logger);
            });
        });

        return app;
    }
}
