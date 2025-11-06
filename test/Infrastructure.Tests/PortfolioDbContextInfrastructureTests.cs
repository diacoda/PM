using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.Infrastructure.Data;
using Xunit;

namespace PM.Infrastructure.Tests
{
    public class PortfolioDbContextInfrastructureTests : IAsyncLifetime
    {
        private readonly DbConnection _connection;
        private readonly DbContextOptions<PortfolioDbContext> _options;

        public PortfolioDbContextInfrastructureTests()
        {
            // Create a single in-memory SQLite connection
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<PortfolioDbContext>()
                .UseSqlite(_connection)
                .Options;
        }

        public async Task InitializeAsync()
        {
            // Ensure database is created for all tests
            await using var context = new PortfolioDbContext(_options);
            await context.Database.EnsureCreatedAsync();
        }

        public Task DisposeAsync()
        {
            _connection.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Can_Create_And_Retrieve_Portfolio_With_Relations()
        {
            await using var context = new PortfolioDbContext(_options);

            // Arrange
            var portfolio = new Portfolio("Integration Test Portfolio");
            var account = new Account("Cash Account", Currency.CAD, FinancialInstitutions.TD);
            var tag = new Tag("Important");
            var holding = new Holding(new Symbol("VFV.TO", "CAD"), 5m);
            var transaction = new Transaction(account.Id, TransactionType.Buy, new Symbol("VFV.TO"), 5m, new Money(500, Currency.CAD), DateOnly.FromDateTime(DateTime.UtcNow));

            account.UpsertHolding(holding);
            account.AddTransaction(transaction);
            account.AddTag(tag);
            portfolio.AddAccount(account);

            context.Portfolios.Add(portfolio);
            await context.SaveChangesAsync();

            // Act
            var retrieved = await context.Portfolios
                .Include(p => p.Accounts)
                    .ThenInclude(a => a.Holdings)
                .Include(p => p.Accounts)
                    .ThenInclude(a => a.Transactions)
                .Include(p => p.Accounts)
                    .ThenInclude(a => a.Tags)
                .FirstAsync();

            // Assert Portfolio
            retrieved.Owner.Should().Be("Integration Test Portfolio");
            retrieved.Accounts.Should().HaveCount(1);

            var retrievedAccount = retrieved.Accounts.First();
            retrievedAccount.Name.Should().Be("Cash Account");
            retrievedAccount.Holdings.Should().HaveCount(1);
            retrievedAccount.Transactions.Should().HaveCount(1);
            retrievedAccount.Tags.Should().HaveCount(1);

            // Assert Holding
            var retrievedHolding = retrievedAccount.Holdings.First();
            retrievedHolding.Quantity.Should().Be(5m);
            retrievedHolding.Asset.Code.Should().Be("VFV.TO");

            // Assert Transaction
            var retrievedTransaction = retrievedAccount.Transactions.First();
            retrievedTransaction.Quantity.Should().Be(5m);
            retrievedTransaction.Symbol.Code.Should().Be("VFV.TO");
            retrievedTransaction.Amount.Amount.Should().Be(500);
            retrievedTransaction.Amount.Currency.Should().Be(Currency.CAD);

            // Assert Tag
            var retrievedTag = retrievedAccount.Tags.First();
            retrievedTag.Name.Should().Be("Important");
        }

        [Fact]
        public async Task Can_Update_And_Delete_Entities()
        {
            await using var context = new PortfolioDbContext(_options);

            // Arrange: create a portfolio
            var portfolio = new Portfolio("Portfolio To Update");
            var account = new Account("Checking", Currency.CAD, FinancialInstitutions.TD);
            portfolio.AddAccount(account);
            context.Portfolios.Add(portfolio);
            await context.SaveChangesAsync();

            // Act: update
            account.Name = "Checking Updated";
            await context.SaveChangesAsync();

            var updated = await context.Accounts.FirstAsync();
            updated.Name.Should().Be("Checking Updated");

            // Act: delete
            context.Portfolios.Remove(portfolio);
            await context.SaveChangesAsync();

            (await context.Portfolios.AnyAsync()).Should().BeFalse();
            (await context.Accounts.AnyAsync()).Should().BeFalse(); // cascade delete works
        }
    }
}
