using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Infrastructure.Data;
using PM.Infrastructure.Repositories;
using PM.SharedKernel;
using Xunit;

namespace PM.Infrastructure.Tests.Repositories
{
    public class PortfolioRepositoryTests : IAsyncLifetime
    {
        private readonly DbConnection _connection;
        private readonly DbContextOptions<PortfolioDbContext> _options;

        public PortfolioRepositoryTests()
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
        public async Task Can_Add_And_Retrieve_Portfolio()
        {
            await using var context = new PortfolioDbContext(_options);
            var repo = new PortfolioRepository(context);

            var portfolio = new Portfolio("TestOwner");
            await repo.AddAsync(portfolio);
            await repo.SaveChangesAsync();

            var retrieved = await repo.GetByIdAsync(portfolio.Id);
            retrieved.Should().NotBeNull();
            retrieved!.Owner.Should().Be("TestOwner");
        }

        [Fact]
        public async Task Can_List_With_Includes()
        {
            await using var context = new PortfolioDbContext(_options);
            var repo = new PortfolioRepository(context);

            var portfolio = new Portfolio("OwnerWithAccounts");
            var account = new Account("Cash", Currency.CAD, PM.Domain.Enums.FinancialInstitutions.TD);
            portfolio.AddAccount(account);

            await repo.AddAsync(portfolio);
            await repo.SaveChangesAsync();

            // Include accounts
            var results = await repo.ListWithIncludesAsync(new[] { IncludeOption.Accounts });
            results.Should().HaveCount(1);
            results.First().Accounts.Should().HaveCount(1);
        }

        [Fact]
        public async Task Can_GetById_With_Multiple_Includes()
        {
            await using var context = new PortfolioDbContext(_options);
            var repo = new PortfolioRepository(context);

            var portfolio = new Portfolio("FullPortfolio");
            var account = new Account("Trading", Currency.CAD, PM.Domain.Enums.FinancialInstitutions.TD);
            var holding = new Holding(new Symbol("VFV.TO"), 10m);
            account.UpsertHolding(holding);
            portfolio.AddAccount(account);

            await repo.AddAsync(portfolio);
            await repo.SaveChangesAsync();

            var retrieved = await repo.GetByIdWithIncludesAsync(
                portfolio.Id,
                new[] { IncludeOption.Accounts, IncludeOption.Holdings, IncludeOption.Transactions });

            retrieved.Should().NotBeNull();
            retrieved!.Accounts.Should().HaveCount(1);
            retrieved.Accounts.First().Holdings.Should().HaveCount(1);
        }

        [Fact]
        public async Task Can_Update_And_Delete_Portfolio()
        {
            await using var context = new PortfolioDbContext(_options);
            var repo = new PortfolioRepository(context);

            var portfolio = new Portfolio("TempPortfolio");
            await repo.AddAsync(portfolio);
            await repo.SaveChangesAsync();

            // Update
            portfolio.Owner = "UpdatedOwner";
            await repo.UpdateAsync(portfolio);
            await repo.SaveChangesAsync();

            var updated = await repo.GetByIdAsync(portfolio.Id);
            updated!.Owner.Should().Be("UpdatedOwner");

            // Delete
            await repo.DeleteAsync(portfolio);
            await repo.SaveChangesAsync();

            var exists = await repo.GetByIdAsync(portfolio.Id);
            exists.Should().BeNull();
        }
    }
}
