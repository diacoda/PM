using Serilog;
using PM.API.Startup;
using PM.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogLogging(builder.Configuration);

builder.Services
    .AddAppConfiguration(builder.Configuration)
    .AddHttpClient()
    .AddDatabase(builder.Configuration, builder.Environment)
    .AddTelemetry(builder.Configuration, builder.Environment)
    .AddHealthChecksWithDependencies(builder.Configuration)
    .AddSymbolConfigs(builder.Configuration)
    .AddProviders()
    .AddRepositories()
    .AddApplicationServices()
    .AddHostedJobs(builder.Configuration)
    .AddSwaggerDocs();

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseGlobalExceptionHandler();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHealthChecksWithUI();
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.MapControllers();
app.Run();