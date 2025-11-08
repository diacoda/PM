using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Infrastructure.Data;
using PM.Infrastructure.Repositories;
using Xunit;
using FluentAssertions;
using PM.Utils.Tests;

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

        private async Task<HoldingRepository> CreateRepositoryWithHoldingsAsync(params Holding[] holdings)
        {
            await using var context = new PortfolioDbContext(_options);
            await context.Holdings.AddRangeAsync(holdings);
            await context.SaveChangesAsync();
            return new HoldingRepository(context);
        }

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ReturnsHolding_WhenExists()
        {
            // Arrange
            var account = TestEntityFactory.CreateAccount("TestAccount", Currency.CAD);
            var tag1 = TestEntityFactory.CreateTag("Tag1");
            var tag2 = TestEntityFactory.CreateTag("Tag2");

            var symbol = new Symbol("AAPL", "USD");
            var holding = new Holding(symbol, 100);
            holding.AddTag(tag1);
            holding.AddTag(tag2);
            holding.AccountId = account.Id;

            var repository = await CreateRepositoryWithHoldingsAsync(holding);

            // Act
            var result = await repository.GetByIdAsync(holding.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(holding.Id);
            result.Asset.Should().Be("AAPL");
            result.Quantity.Should().Be(100);
            result.Tags.Should().HaveCount(2);
            result.Tags.Should().Contain(t => t.Name == "Tag1");
            result.Tags.Should().Contain(t => t.Name == "Tag2");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var repository = await CreateRepositoryWithHoldingsAsync();

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region ListByAccountAsync

        [Fact]
        public async Task ListByAccountAsync_ReturnsAllHoldings_ForGivenAccount()
        {
            // Arrange
            var account1 = TestEntityFactory.CreateAccount("Account1", Currency.CAD);
            var account2 = TestEntityFactory.CreateAccount("Account2", Currency.USD);

            var holdings = new[]
            {
                new Holding(new Symbol("AAPL", "USD"), 50) { AccountId = account1.Id },
                new Holding(new Symbol("MSFT", "USD"), 30) { AccountId = account1.Id },
                new Holding(new Symbol("GOOG", "USD"), 10) { AccountId = account2.Id }
            };

            var repository = await CreateRepositoryWithHoldingsAsync(holdings);

            // Act
            var result = await repository.ListByAccountAsync(account1.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(h => h.AccountId.Should().Be(account1.Id));
            result.Select(h => h.Asset.Code).Should().Contain(new[] { "AAPL", "MSFT" });
        }

        [Fact]
        public async Task ListByAccountAsync_ReturnsEmpty_WhenNoHoldingsForAccount()
        {
            // Arrange
            var repository = await CreateRepositoryWithHoldingsAsync();

            // Act
            var result = await repository.ListByAccountAsync(123);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion
    }
}
