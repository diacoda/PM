using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace PM.API.Configuration;

public static class SwaggerConfig
{
    public static IServiceCollection AddSwaggerWithVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddSwaggerGen();
        services.ConfigureOptions<SwaggerVersioningOptions>();
        return services;
    }

    public static WebApplication UseSwaggerIfDevelopment(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var desc in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",
                    desc.GroupName.ToUpperInvariant());
            }
        });
        return app;
    }
}
