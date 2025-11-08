using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PM.Infrastructure.Data;

namespace PM.API.Startup;

/// <summary>
/// Provides an extension method to configure SQLite in-memory databases
/// for use during integration testing.
/// </summary>
/// <remarks>
/// <para>
/// During integration tests, each EF Core DbContext (Portfolio, CashFlow, Valuation)
/// is configured to use an independent in-memory SQLite database.  
/// Unlike UseInMemoryDatabase(),
/// SQLite in-memory mode runs through the full relational EF Core stack, ensuring that
/// migrations, schema constraints, and SQL translations are all exercised as they would be
/// against a real database.
/// </para>
/// <para>
/// Each connection is explicitly opened and kept alive for the duration of the test host,
/// which preserves data across multiple DbContext instances within the same test run.
/// </para>
/// </remarks>
public static class IntegrationTestDatabaseExtensions
{
    /// <summary>
    /// Registers and initializes persistent in-memory SQLite databases for integration testing.
    /// </summary>
    /// <param name="services">
    /// The service collection used to register the EF Core database contexts.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance, allowing further configuration chaining.
    /// </returns>
    /// <example>
    /// Typical usage in a custom <c>WebApplicationFactory</c>:
    /// <code>
    /// builder.Services.AddInMemorySqliteDatabases();
    /// </code>
    /// </example>
    /// <remarks>
    /// <list type="number">
    /// <item>
    /// <description>Creates one persistent <see cref="SqliteConnection"/> per DbContext type.</description>
    /// </item>
    /// <item>
    /// <description>Opens each connection so that the in-memory database remains alive as long as the connection is open.</description>
    /// </item>
    /// <item>
    /// <description>Configures each DbContext to use its corresponding SQLite connection.</description>
    /// </item>
    /// <item>
    /// <description>Ensures that the schema for each context is created immediately by calling EnsureCreated().</description>
    /// </item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddInMemorySqliteDatabases(this IServiceCollection services)
    {
        // 1️⃣ Create dedicated, persistent in-memory SQLite connections for each context.
        var portfolioConnection = new SqliteConnection("DataSource=:memory:");
        var cashFlowConnection = new SqliteConnection("DataSource=:memory:");
        var valuationConnection = new SqliteConnection("DataSource=:memory:");

        // 2️⃣ Open all connections to keep the in-memory databases alive
        // throughout the lifetime of the test host.
        portfolioConnection.Open();
        cashFlowConnection.Open();
        valuationConnection.Open();

        // 3️⃣ Register each DbContext to use its corresponding open connection.
        services.AddDbContext<PortfolioDbContext>(options =>
            options.UseSqlite(portfolioConnection));

        services.AddDbContext<CashFlowDbContext>(options =>
            options.UseSqlite(cashFlowConnection));

        services.AddDbContext<ValuationDbContext>(options =>
            options.UseSqlite(valuationConnection));

        // 4️⃣ Build a temporary service provider to create and initialize database schemas.
        using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();

        // 5️⃣ Ensure the schema is created for each in-memory database.
        scope.ServiceProvider.GetRequiredService<PortfolioDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<CashFlowDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<ValuationDbContext>().Database.EnsureCreated();

        return services;
    }
}
