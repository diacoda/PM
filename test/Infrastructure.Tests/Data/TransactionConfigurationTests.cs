using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.Infrastructure.Data;
using PM.Infrastructure.Data.Configurations;
using Xunit;

namespace PM.Infrastructure.Data.Tests
{
    public class TransactionConfigurationTests : IAsyncLifetime
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<PortfolioDbContext> _options;

        public TransactionConfigurationTests()
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
        public async Task TransactionConfiguration_Should_Persist_With_Account_And_Portfolio()
        {
            await using var context = new PortfolioDbContext(_options);

            // Arrange: create portfolio, account, and transaction
            var portfolio = new Portfolio("MyPortfolio");

            await context.Portfolios.AddAsync(portfolio);
            await context.SaveChangesAsync();

            var account = new Account("RRSP", new Currency("CAD"), FinancialInstitutions.TD);
            portfolio.AddAccount(account);
            await context.Accounts.AddAsync(account);
            await context.SaveChangesAsync();

            var symbol = new Symbol("VFV.TO", "CAD");
            var amount = new Money(1000m, new Currency("CAD"));
            var costs = new Money(10m, new Currency("CAD"));

            var transaction = new Transaction
            {
                Date = new DateOnly(2025, 1, 1),
                Type = TransactionType.Buy,
                Symbol = symbol,
                Quantity = 50,
                Amount = amount,
                Costs = costs,
                AccountId = account.Id
            };

            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            // Act: retrieve it back including Account and Portfolio
            var stored = await context.Transactions
                .Include(t => t.Account)
                .ThenInclude(a => a!.Portfolio) // use null-forgiving operator here
                .FirstOrDefaultAsync();

            // Assert: transaction top-level
            stored.Should().NotBeNull();
            stored!.Date.Should().Be(transaction.Date);
            stored.Type.Should().Be(transaction.Type);
            stored.Quantity.Should().Be(transaction.Quantity);
            stored.AccountId.Should().Be(account.Id);

            // Assert: symbol owned type
            stored.Symbol.Should().NotBeNull();
            stored.Symbol.Code.Should().Be(symbol.Code);
            stored.Symbol.Currency.Code.Should().Be(symbol.Currency.Code);
            stored.Symbol.AssetClass.Should().Be(symbol.AssetClass);

            // Assert: amount owned type
            stored.Amount.Should().NotBeNull();
            stored.Amount.Amount.Should().Be(amount.Amount);
            stored.Amount.Currency.Code.Should().Be(amount.Currency.Code);

            // Assert: optional costs
            stored.Costs.Should().NotBeNull();
            stored.Costs!.Amount.Should().Be(costs.Amount);
            stored.Costs.Currency.Code.Should().Be(costs.Currency.Code);

            // Assert: account and portfolio
            stored.Account.Should().NotBeNull();
            var accRef = stored.Account!; // safe local reference
            accRef.Id.Should().Be(account.Id);

            accRef.Portfolio.Should().NotBeNull();
            var portRef = accRef.Portfolio!;
            portRef.Owner.Should().Be(portfolio.Owner);
        }

    }
}
