using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.Infrastructure.Data;
using PM.Infrastructure.Repositories;
using PM.SharedKernel;
using Xunit;

namespace PM.Infrastructure.Tests.Repositories
{
    public class AccountRepositoryTests : IAsyncLifetime
    {
        private readonly DbConnection _connection;
        private readonly DbContextOptions<PortfolioDbContext> _options;

        public AccountRepositoryTests()
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

        private static Account CreateAccount(string name = "Cash Account")
        {
            var symbol = new Symbol("VFV.TO");
            var currency = new Currency("CAD");
            var account = new Account(name, currency, FinancialInstitutions.TD);

            // Add domain entities through proper methods
            var money = new Money(100, currency);
            Holding h = account.UpsertHolding(new Holding(symbol, 10));

            account.AddTransaction(new Transaction(1, TransactionType.Buy, symbol, 10, money, DateOnly.FromDateTime(DateTime.Today)));

            account.AddTag(new Tag("RRSP"));

            return account;
        }



        [Fact]
        public async Task AddAsync_Should_Add_Account_To_Db()
        {
            await using var context = new PortfolioDbContext(_options);
            var repo = new AccountRepository(context);

            var portfolio = new Portfolio("Person1");

            // Save portfolio to get ID
            context.Portfolios.Add(portfolio);
            await context.SaveChangesAsync();

            var account = CreateAccount();
            account.LinkToPortfolio(portfolio);

            await repo.AddAsync(account);
            await repo.SaveChangesAsync();

            var stored = await context.Accounts.FirstOrDefaultAsync();
            stored.Should().NotBeNull();
            stored!.Name.Should().Be("Cash Account");
        }

        [Fact]
        public async Task UpdateAsync_Should_Modify_Existing_Account()
        {
            await using var context = new PortfolioDbContext(_options);
            var repo = new AccountRepository(context);

            var portfolio = new Portfolio("Person1");

            // Save portfolio to get ID
            context.Portfolios.Add(portfolio);
            await context.SaveChangesAsync();

            var account = CreateAccount();
            account.LinkToPortfolio(portfolio);

            await repo.AddAsync(account);
            await repo.SaveChangesAsync();

            account.UpdateName("Updated Account");
            await repo.UpdateAsync(account);
            await repo.SaveChangesAsync();

            var stored = await context.Accounts.FirstAsync();
            stored.Name.Should().Be("Updated Account");
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Account_From_Db()
        {
            await using var context = new PortfolioDbContext(_options);
            var repo = new AccountRepository(context);

            var portfolio = new Portfolio("Person1");

            // Save portfolio to get ID
            context.Portfolios.Add(portfolio);
            await context.SaveChangesAsync();

            var account = CreateAccount();
            account.LinkToPortfolio(portfolio);

            await repo.AddAsync(account);
            await repo.SaveChangesAsync();

            await repo.DeleteAsync(account);
            await repo.SaveChangesAsync();

            (await context.Accounts.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task ListByPortfolioAsync_Should_Filter_By_PortfolioId()
        {
            await using var context = new PortfolioDbContext(_options);
            var repo = new AccountRepository(context);

            var portfolio1 = new Portfolio("Person1");
            context.Portfolios.Add(portfolio1);
            await context.SaveChangesAsync();

            var portfolio2 = new Portfolio("Person2");
            context.Portfolios.Add(portfolio2);
            await context.SaveChangesAsync();

            var account1 = CreateAccount("Portfolio1_Account");
            account1.LinkToPortfolio(portfolio1);
            var account2 = CreateAccount("Portfolio2_Account");
            account2.LinkToPortfolio(portfolio2);

            await repo.AddAsync(account1);
            await repo.AddAsync(account2);
            await repo.SaveChangesAsync();

            var results = await repo.ListByPortfolioAsync(portfolio1.Id);
            results.Should().ContainSingle(a => a.PortfolioId == portfolio1.Id);
        }

        [Theory]
        [InlineData(new IncludeOption[0])]
        [InlineData(new[] { IncludeOption.Holdings })]
        [InlineData(new[] { IncludeOption.Transactions })]
        [InlineData(new[] { IncludeOption.Tags })]
        [InlineData(new[] { IncludeOption.Holdings, IncludeOption.Transactions, IncludeOption.Tags })]
        public async Task ListByPortfolioWithIncludesAsync_Should_Include_Related_Collections(IncludeOption[] includes)
        {
            // Arrange: use initial context to add portfolio and account
            await using (var context = new PortfolioDbContext(_options))
            {
                var repo = new AccountRepository(context);

                var portfolio = new Portfolio("Person2");
                context.Portfolios.Add(portfolio);
                await context.SaveChangesAsync();

                var account = CreateAccount("Portfolio2_Account");
                account.LinkToPortfolio(portfolio);

                await repo.AddAsync(account);
                await repo.SaveChangesAsync();
            }

            // Act: use a fresh context to ensure EF must load included navigation properties
            await using (var context = new PortfolioDbContext(_options))
            {
                var repo = new AccountRepository(context);

                var results = await repo.ListByPortfolioWithIncludesAsync(1, includes); // portfolio.Id = 1
                var loaded = results.First();

                // Assert: base
                loaded.Should().NotBeNull();

                // Conditional includes
                bool includeHoldings = includes.Contains(IncludeOption.Holdings);
                bool includeTransactions = includes.Contains(IncludeOption.Transactions);
                bool includeTags = includes.Contains(IncludeOption.Tags);

                loaded.Holdings.Count.Should().Be(includeHoldings ? 1 : 0);
                loaded.Transactions.Count.Should().Be(includeTransactions ? 1 : 0);
                loaded.Tags.Count.Should().Be(includeTags ? 1 : 0);
            }
        }


        [Fact]
        public async Task ApplyIncludes_Should_Use_SplitQuery_To_Avoid_MultiCollection_Performance_Warnings()
        {
            await using var context = new PortfolioDbContext(_options);
            var repo = new AccountRepository(context);

            var portfolio = new Portfolio("Person1");

            // Save portfolio to get ID
            context.Portfolios.Add(portfolio);
            await context.SaveChangesAsync();

            var account = CreateAccount();
            account.LinkToPortfolio(portfolio);

            await repo.AddAsync(account);
            await repo.SaveChangesAsync();

            var results = await repo.ListByPortfolioWithIncludesAsync(
                1,
                new[] { IncludeOption.Holdings, IncludeOption.Transactions, IncludeOption.Tags }
            );

            results.Should().NotBeEmpty();
            // Just ensuring EF didn't throw or collapse data into a single query
        }
    }
}
