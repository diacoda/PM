using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;
using PM.Domain.Values;
namespace PM.Infrastructure.Tests;

public class PortfolioConfigurationTests
{
    private DbContextOptions<PortfolioDbContext> Options =>
        new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

    [Fact]
    public async Task Can_Add_And_Retrieve_Portfolio()
    {
        await using var context = new PortfolioDbContext(Options);

        var portfolio = new Portfolio("Owner1");
        context.Portfolios.Add(portfolio);
        await context.SaveChangesAsync();

        var retrieved = await context.Portfolios.FirstAsync();
        retrieved.Owner.Should().Be("Owner1");
    }

    [Fact]
    public void Cannot_Create_Portfolio_With_Null_Owner()
    {
        Action act = () => new Portfolio(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task Cascade_Delete_Removes_Accounts()
    {
        await using var context = new PortfolioDbContext(Options);

        var portfolio = new Portfolio("Owner2");
        var account = new Account("Checking", Currency.CAD, PM.Domain.Enums.FinancialInstitutions.TD);
        portfolio.AddAccount(account);

        context.Portfolios.Add(portfolio);
        await context.SaveChangesAsync();

        context.Portfolios.Remove(portfolio);
        await context.SaveChangesAsync();

        var accounts = await context.Accounts.ToListAsync();
        accounts.Should().BeEmpty();
    }
}
