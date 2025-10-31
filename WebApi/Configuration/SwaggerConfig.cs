using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace PM.API.Configuration;

public static class SwaggerConfig
{
    public static IServiceCollection AddSwaggerDocs(this IServiceCollection services)
    {
        services.AddOpenApiDocument(options =>
        {
            options.PostProcess = document =>
            {
                document.Info = new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Portfolio Management API"
                };
            };
        });

        return services;
    }
}
