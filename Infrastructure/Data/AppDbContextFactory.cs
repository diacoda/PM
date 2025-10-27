using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PM.Infrastructure.Configuration;

namespace PM.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private readonly IConfiguration? _configuration;

    /// <summary>
    /// Optional constructor to pass configuration from DI or WebApi.
    /// </summary>
    public AppDbContextFactory(IConfiguration? configuration = null)
    {
        _configuration = configuration;
    }
    public AppDbContextFactory() { } // EF CLI will use this
    public AppDbContext CreateDbContext(string[] args)
    {
        // Use the passed configuration if available (WebApi runtime)
        // Otherwise, build a minimal config (EF CLI or fallback)
        var configuration = _configuration ?? BuildMinimalConfiguration();

        var absolutePath = DatabasePathResolver.ResolveAbsolutePath(configuration);
        var connString = DatabasePathResolver.BuildSqliteConnectionString(absolutePath);

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite(connString);

        return new AppDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Builds a minimal configuration for EF CLI scenarios.
    /// </summary>
    private static IConfiguration BuildMinimalConfiguration()
    {
        // Only load the "Database:RelativePath" section, if it exists
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true) // optional
            .Build();
    }
}
