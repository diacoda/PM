using System;
using System.Collections.Generic;
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
using Xunit;

namespace PM.Infrastructure.Repositories.Tests
{
    public class PriceRepositoryTests : IAsyncLifetime
    {
        private readonly DbConnection _connection;
        private readonly DbContextOptions<ValuationDbContext> _options;

        public PriceRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<ValuationDbContext>()
                .UseSqlite((SqliteConnection)_connection)
                .Options;
        }

        public async Task InitializeAsync()
        {
            // Ensure DB schema exists for tests
            await using var context = new ValuationDbContext(_options);
            await context.Database.EnsureCreatedAsync();
        }

        public Task DisposeAsync()
        {
            _connection.Dispose();
            return Task.CompletedTask;
        }

        // Helper to construct an AssetPrice
        private static AssetPrice CreatePrice(string symbolCode, string currencyCode, DateOnly date, decimal amount, string source = "UnitTest")
        {
            var symbol = new Symbol(symbolCode, currencyCode);
            var money = new Money(amount, new Currency(currencyCode));
            return new AssetPrice(symbol, date, money, source);
        }

        [Fact]
        public async Task SaveAsync_Should_Insert_Price()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new PriceRepository(context);

            var p = CreatePrice("VFV.TO", "CAD", new DateOnly(2025, 1, 1), 100m);
            await repo.SaveAsync(p);

            var stored = await context.Prices.AsNoTracking().SingleAsync();
            stored.Should().NotBeNull();
            stored.Symbol.Code.Should().Be("VFV.TO");
            stored.Price.Amount.Should().Be(100m);
            stored.Date.Should().Be(new DateOnly(2025, 1, 1));
        }

        [Fact]
        public async Task GetAsync_ReturnsPrice_WhenExists()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new PriceRepository(context);

            var p = CreatePrice("VFV.TO", "CAD", new DateOnly(2025, 1, 2), 110m);
            await context.Prices.AddAsync(p);
            await context.SaveChangesAsync();

            var fetched = await repo.GetAsync(new Symbol("VFV.TO", "CAD"), new DateOnly(2025, 1, 2));
            fetched.Should().NotBeNull();
            fetched!.Price.Amount.Should().Be(110m);
        }

        [Fact]
        public async Task UpsertAsync_Should_Update_ExistingPrice_Not_InsertDuplicate()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new PriceRepository(context);

            // Arrange
            var original = CreatePrice("VFV.TO", "CAD", new DateOnly(2025, 1, 1), 100m);
            await repo.UpsertAsync(original);

            // Act
            var updated = CreatePrice("VFV.TO", "CAD", new DateOnly(2025, 1, 1), 150m);
            await repo.UpsertAsync(updated);

            // ✅ Assert using a fresh context to see persisted state
            await using var verificationContext = new ValuationDbContext(_options);
            var stored = await verificationContext.Prices.AsNoTracking().SingleAsync();

            stored.Price.Amount.Should().Be(150m);

        }

        [Fact]
        public async Task GetAllForSymbolAsync_Returns_Prices_InDateOrder()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new PriceRepository(context);

            var p1 = CreatePrice("VFV.TO", "CAD", new DateOnly(2025, 1, 1), 100m);
            var p2 = CreatePrice("VFV.TO", "CAD", new DateOnly(2025, 1, 3), 120m);
            var p3 = CreatePrice("VFV.TO", "CAD", new DateOnly(2025, 1, 2), 110m);

            await context.Prices.AddRangeAsync(p1, p2, p3);
            await context.SaveChangesAsync();

            var list = await repo.GetAllForSymbolAsync(new Symbol("VFV.TO", "CAD"));
            list.Should().HaveCount(3);
            list.Select(p => p.Date).Should().BeInAscendingOrder();
            list.Select(p => p.Price.Amount).Should().ContainInOrder(new[] { 100m, 110m, 120m });
        }

        [Fact]
        public async Task DeleteAsync_RemovesPrice_WhenExists()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new PriceRepository(context);

            var p = CreatePrice("VFV.TO", "CAD", new DateOnly(2025, 2, 1), 200m);
            await context.Prices.AddAsync(p);
            await context.SaveChangesAsync();

            var removed = await repo.DeleteAsync(new Symbol("VFV.TO", "CAD"), new DateOnly(2025, 2, 1));
            removed.Should().BeTrue();

            var fetched = await context.Prices.FirstOrDefaultAsync();
            fetched.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new PriceRepository(context);

            var removed = await repo.DeleteAsync(new Symbol("VFV.TO", "CAD"), new DateOnly(2025, 3, 1));
            removed.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllByDateAsync_Returns_Prices_OrderedByAmount()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new PriceRepository(context);

            var date = new DateOnly(2025, 4, 4);
            var a = CreatePrice("VFV.TO", "CAD", date, 300m);
            var b = CreatePrice("VCE.TO", "CAD", date, 100m);
            var c = CreatePrice("HXQ.TO", "CAD", date, 200m);

            await context.Prices.AddRangeAsync(a, b, c);
            await context.SaveChangesAsync();

            var list = await repo.GetAllByDateAsync(date);
            list.Should().HaveCount(3);
            list.Select(x => x.Price.Amount).Should().ContainInOrder(new[] { 100m, 200m, 300m });
        }
    }
}
