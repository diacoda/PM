using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace PM.API.Startup
{
    /// <summary>
    /// Provides extension methods to register OpenAPI/Swagger documentation for the API.
    /// </summary>
    /// <remarks>
    /// Centralizes registration of NSwag OpenAPI document generation and configuration.
    /// </remarks>
    public static class SwaggerConfig
    {
        /// <summary>
        /// Adds OpenAPI/Swagger document generation to the DI container.
        /// </summary>
        /// <param name="services">The DI service collection.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
        /// <example>
        /// Usage in <c>Program.cs</c>:
        /// <code>
        /// builder.Services.AddSwaggerDocs();
        /// </code>
        /// </example>
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

                // Optional: configure security schemes, enum descriptions, etc.
                // options.SchemaSettings.GenerateEnumMappingDescription = true;
            });

            return services;
        }
    }
}
