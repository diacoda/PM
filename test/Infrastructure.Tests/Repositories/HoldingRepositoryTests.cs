using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Infrastructure.Data;
using PM.Infrastructure.Repositories;
using Xunit;
using FluentAssertions;
using PM.Utils.Tests;
using PM.Domain.Enums;

namespace PM.Infrastructure.Tests.Repositories
{
    public class HoldingRepositoryTests : IAsyncLifetime
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<PortfolioDbContext> _options;

        public HoldingRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<PortfolioDbContext>()
                .UseSqlite(_connection)
                .Options;
        }

        public async Task InitializeAsync()
        {
            await using var context = new PortfolioDbContext(_options);
            await context.Database.EnsureCreatedAsync();
        }

        public Task DisposeAsync()
        {
            _connection.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsHoldingWithTags_WhenExists()
        {
            // Arrange
            await using var context = new PortfolioDbContext(_options);

            // 1️⃣ Create and save a Portfolio
            var portfolio = new Portfolio("My Portfolio");
            await context.Portfolios.AddAsync(portfolio);
            await context.SaveChangesAsync();

            // 2️⃣ Create an Account linked to that Portfolio
            var account = new Account("TestAccount", Currency.CAD, FinancialInstitutions.TD);
            account.LinkToPortfolio(portfolio);

            await context.Accounts.AddAsync(account);
            await context.SaveChangesAsync();

            // 3️⃣ Create and save Tags
            var tag1 = new Tag("Tag1");
            var tag2 = new Tag("Tag2");
            await context.Tags.AddRangeAsync(tag1, tag2);
            await context.SaveChangesAsync();

            // 4️⃣ Create and save a Holding linked to the Account
            var symbol = new Symbol("VFV.TO", "CAD");
            var holding = new Holding(symbol, 100)
            {
                AccountId = account.Id
            };
            holding.AddTag(tag1);
            holding.AddTag(tag2);
            await context.Holdings.AddAsync(holding);
            await context.SaveChangesAsync();

            // 5️⃣ Create repository
            var repository = new HoldingRepository(context);

            // Act
            var result = await repository.GetByIdAsync(holding.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(holding.Id);
            result.Asset.Should().Be(symbol);
            result.Quantity.Should().Be(100);
            result.AccountId.Should().Be(account.Id);
            result.Tags.Should().HaveCount(2);
            result.Tags.Select(t => t.Name).Should().Contain(new[] { "Tag1", "Tag2" });
        }



        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            await using var context = new PortfolioDbContext(_options);
            var repository = new HoldingRepository(context);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ListByAccountAsync_ReturnsAllHoldings_ForGivenAccount()
        {
            // Arrange
            await using var context = new PortfolioDbContext(_options);

            // 1️⃣ Create and save two portfolios
            var portfolio1 = new Portfolio("Portfolio 1");
            var portfolio2 = new Portfolio("Portfolio 2");
            await context.Portfolios.AddRangeAsync(portfolio1, portfolio2);
            await context.SaveChangesAsync();

            // 2️⃣ Create accounts linked to portfolios
            var account1 = TestEntityFactory.CreateAccount("Account1", Currency.CAD);
            account1.LinkToPortfolio(portfolio1);

            var account2 = TestEntityFactory.CreateAccount("Account2", Currency.USD);
            account2.LinkToPortfolio(portfolio2);

            await context.Accounts.AddRangeAsync(account1, account2);
            await context.SaveChangesAsync();

            // 3️⃣ Create holdings and associate them with accounts
            var h1 = TestEntityFactory.CreateHolding(new Symbol("VFV.TO", "CAD"), 100);
            h1.AccountId = account1.Id;

            var h2 = TestEntityFactory.CreateHolding(new Symbol("VCE.TO", "USD"), 50);
            h2.AccountId = account1.Id;

            var h3 = TestEntityFactory.CreateHolding(new Symbol("HXQ.TO", "USD"), 10);
            h3.AccountId = account2.Id;

            await context.Holdings.AddRangeAsync(h1, h2, h3);
            await context.SaveChangesAsync();

            // 4️⃣ Create repository
            var repository = new HoldingRepository(context);

            // Act
            var result = await repository.ListByAccountAsync(account1.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(h => h.AccountId.Should().Be(account1.Id));
            result.Select(h => h.Asset.Code).Should().Contain(new[] { "VFV.TO", "VCE.TO" });
        }
        [Fact]
        public async Task ListByAccountAsync_ReturnsEmpty_WhenNoHoldingsForAccount()
        {
            // Arrange
            await using var context = new PortfolioDbContext(_options);
            var repository = new HoldingRepository(context);

            // Act
            var result = await repository.ListByAccountAsync(123);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
