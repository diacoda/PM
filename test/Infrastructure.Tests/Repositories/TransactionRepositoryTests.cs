using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using PM.Infrastructure.Data;
using PM.Infrastructure.Repositories;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Domain.Enums;
using Xunit;

namespace PM.Infrastructure.Repositories.Tests
{
    public class TransactionRepositoryTests : IAsyncLifetime
    {
        private readonly DbConnection _connection;
        private readonly DbContextOptions<PortfolioDbContext> _options;

        public TransactionRepositoryTests()
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
        public async Task AddAsync_ShouldAddTransaction()
        {
            // Arrange
            await using var context = new PortfolioDbContext(_options);
            var repo = new TransactionRepository(context);

            var portfolio = new Portfolio("Portfolio1");
            await context.Portfolios.AddAsync(portfolio);
            await context.SaveChangesAsync();

            var account = new Account("Account1", Currency.CAD, FinancialInstitutions.TD);
            account.LinkToPortfolio(portfolio);
            await context.Accounts.AddAsync(account);
            await context.SaveChangesAsync();

            var transaction = new Transaction(
                account.Id,
                TransactionType.Deposit,
                new Symbol("VFV.TO", "CAD"),
                100m,
                new Money(1000m, Currency.CAD),
                DateOnly.FromDateTime(DateTime.UtcNow));

            // Act
            await repo.AddAsync(transaction);
            await repo.SaveChangesAsync();

            // Assert
            var stored = await context.Transactions.FirstOrDefaultAsync();
            stored.Should().NotBeNull();
            stored!.Id.Should().Be(transaction.Id);
            stored.AccountId.Should().Be(account.Id);
            stored.Symbol.Should().Be(transaction.Symbol);
            stored.Quantity.Should().Be(100m);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTransaction_WhenExists()
        {
            // Arrange
            await using var context = new PortfolioDbContext(_options);
            var repo = new TransactionRepository(context);

            var portfolio = new Portfolio("Portfolio2");
            await context.Portfolios.AddAsync(portfolio);

            var account = new Account("Account2", Currency.CAD, FinancialInstitutions.TD);
            account.LinkToPortfolio(portfolio);
            await context.Accounts.AddAsync(account);
            await context.SaveChangesAsync();

            var transaction = new Transaction(
                account.Id,
                TransactionType.Deposit,
                new Symbol("VCE.TO", "CAD"),
                50m,
                new Money(1000m, Currency.CAD),
                DateOnly.FromDateTime(DateTime.UtcNow));
            await context.Transactions.AddAsync(transaction);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync(transaction.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(transaction.Id);
            result.Symbol.Should().Be(transaction.Symbol);
            result.Quantity.Should().Be(50m);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            await using var context = new PortfolioDbContext(_options);
            var repo = new TransactionRepository(context);

            var result = await repo.GetByIdAsync(999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task ListByAccountAsync_ShouldReturnTransactions_ForGivenAccount()
        {
            // Arrange
            await using var context = new PortfolioDbContext(_options);
            var repo = new TransactionRepository(context);

            var portfolio = new Portfolio("Portfolio3");
            await context.Portfolios.AddAsync(portfolio);

            var account1 = new Account("Account1", Currency.CAD, FinancialInstitutions.TD);
            var account2 = new Account("Account2", Currency.USD, FinancialInstitutions.TD);
            account1.LinkToPortfolio(portfolio);
            account2.LinkToPortfolio(portfolio);
            await context.Accounts.AddRangeAsync(account1, account2);
            await context.SaveChangesAsync();

            var t1 = new Transaction(
                account1.Id,
                TransactionType.Deposit,
                new Symbol("VFV.TO", "CAD"),
                100m,
                new Money(1000m, Currency.CAD),
                DateOnly.FromDateTime(DateTime.UtcNow));
            var t2 = new Transaction(
                account1.Id,
                TransactionType.Deposit,
                new Symbol("VCE.TO", "CAD"),
                50m,
                new Money(1000m, Currency.CAD),
                DateOnly.FromDateTime(DateTime.UtcNow));
            var t3 = new Transaction(
                account2.Id,
                TransactionType.Deposit,
                new Symbol("HXQ.TO", "CAD"),
                10m,
                new Money(1000m, Currency.CAD),
                DateOnly.FromDateTime(DateTime.UtcNow));

            await context.Transactions.AddRangeAsync(t1, t2, t3);
            await context.SaveChangesAsync();

            // Act
            var results = await repo.ListByAccountAsync(account1.Id);

            // Assert
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(t => t.AccountId.Should().Be(account1.Id));
            results.Select(t => t.Symbol.Code).Should().Contain(new[] { "VFV.TO", "VCE.TO" });
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveTransaction()
        {
            // Arrange
            await using var context = new PortfolioDbContext(_options);
            var repo = new TransactionRepository(context);

            var portfolio = new Portfolio("Portfolio4");
            await context.Portfolios.AddAsync(portfolio);

            var account = new Account("Account1", Currency.CAD, FinancialInstitutions.TD);
            account.LinkToPortfolio(portfolio);
            await context.Accounts.AddAsync(account);
            await context.SaveChangesAsync();

            var transaction = new Transaction(
                account.Id,
                TransactionType.Deposit,
                new Symbol("VFV.TO", "CAD"),
                100m,
                new Money(1000m, Currency.CAD),
                DateOnly.FromDateTime(DateTime.UtcNow));
            await context.Transactions.AddAsync(transaction);
            await context.SaveChangesAsync();

            // Act
            await repo.DeleteAsync(transaction);
            await repo.SaveChangesAsync();

            // Assert
            var stored = await context.Transactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);
            stored.Should().BeNull();
        }
    }
}
