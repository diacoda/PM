using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PM.Infrastructure.Data;

namespace PM.API.Startup;

public static class IntegrationTestDatabaseExtensions
{
    public static IServiceCollection AddInMemorySqliteDatabases(this IServiceCollection services)
    {
        // One persistent open connection per context type
        var portfolioConnection = new SqliteConnection("DataSource=:memory:");
        var cashFlowConnection = new SqliteConnection("DataSource=:memory:");
        var valuationConnection = new SqliteConnection("DataSource=:memory:");

        portfolioConnection.Open();
        cashFlowConnection.Open();
        valuationConnection.Open();

        services.AddDbContext<PortfolioDbContext>(options =>
            options.UseSqlite(portfolioConnection));

        services.AddDbContext<CashFlowDbContext>(options =>
            options.UseSqlite(cashFlowConnection));

        services.AddDbContext<ValuationDbContext>(options =>
            options.UseSqlite(valuationConnection));

        // Ensure schema is created
        using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        scope.ServiceProvider.GetRequiredService<PortfolioDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<CashFlowDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<ValuationDbContext>().Database.EnsureCreated();

        return services;
    }
}
