using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PM.Domain.Values;
using PM.Infrastructure.Data;
using PM.Infrastructure.Repositories;
using Xunit;

namespace PM.Infrastructure.Repositories.Tests
{
    public class FxRateRepositoryTests
    {
        private static ValuationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ValuationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // isolated per test
                .Options;

            return new ValuationDbContext(options);
        }

        [Fact]
        public async Task SaveAsync_AddsRateToDatabase()
        {
            using var db = CreateDbContext();
            var repo = new FxRateRepository(db);

            var rate = new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10), 1.35m);

            await repo.SaveAsync(rate);

            var saved = await db.FxRates.FirstOrDefaultAsync();
            saved.Should().NotBeNull();
            saved!.Rate.Should().Be(1.35m);
            saved.FromCurrency.Should().Be(Currency.USD);
            saved.ToCurrency.Should().Be(Currency.CAD);
        }

        [Fact]
        public async Task GetAsync_ReturnsExistingRate()
        {
            using var db = CreateDbContext();
            var repo = new FxRateRepository(db);
            var rate = new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10), 1.35m);
            db.FxRates.Add(rate);
            await db.SaveChangesAsync();

            var found = await repo.GetAsync(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10));

            found.Should().NotBeNull();
            found!.Rate.Should().Be(1.35m);
        }

        [Fact]
        public async Task GetAsync_ReturnsNull_WhenNotFound()
        {
            using var db = CreateDbContext();
            var repo = new FxRateRepository(db);

            var result = await repo.GetAsync(Currency.EUR, Currency.CAD, new DateOnly(2024, 05, 10));

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpsertAsync_UpdatesExistingRate()
        {
            using var db = CreateDbContext();
            var repo = new FxRateRepository(db);
            var rate = new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10), 1.35m);
            db.FxRates.Add(rate);
            await db.SaveChangesAsync();

            var updated = new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10), 1.40m);
            await repo.UpsertAsync(updated);

            var saved = await db.FxRates.FirstAsync();
            saved.Rate.Should().Be(1.40m);
        }

        [Fact]
        public async Task UpsertAsync_InsertsNewRate_WhenNotExists()
        {
            using var db = CreateDbContext();
            var repo = new FxRateRepository(db);

            var newRate = new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 11), 1.38m);
            await repo.UpsertAsync(newRate);

            var all = await db.FxRates.ToListAsync();
            all.Should().HaveCount(1);
            all[0].Rate.Should().Be(1.38m);
        }

        [Fact]
        public async Task GetAllForPairAsync_ReturnsOrderedRates()
        {
            using var db = CreateDbContext();
            var repo = new FxRateRepository(db);
            var rates = new[]
            {
                new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 12), 1.40m),
                new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10), 1.35m),
                new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 11), 1.38m),
            };
            db.FxRates.AddRange(rates);
            await db.SaveChangesAsync();

            var list = await repo.GetAllForPairAsync(Currency.USD, Currency.CAD);

            list.Should().HaveCount(3);
            list.Should().BeInAscendingOrder(f => f.Date);
        }

        [Fact]
        public async Task DeleteAsync_RemovesExistingRate()
        {
            using var db = CreateDbContext();
            var repo = new FxRateRepository(db);
            var rate = new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10), 1.35m);
            db.FxRates.Add(rate);
            await db.SaveChangesAsync();

            var deleted = await repo.DeleteAsync(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10));

            deleted.Should().BeTrue();
            (await db.FxRates.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            using var db = CreateDbContext();
            var repo = new FxRateRepository(db);

            var deleted = await repo.DeleteAsync(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10));

            deleted.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllByDateAsync_ReturnsOrderedByCurrencyCodes()
        {
            using var db = CreateDbContext();
            var repo = new FxRateRepository(db);
            db.FxRates.AddRange(new[]
            {
                new FxRate(Currency.EUR, Currency.CAD, new DateOnly(2024, 05, 10), 1.48m),
                new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10), 1.35m),
            });
            await db.SaveChangesAsync();

            var list = await repo.GetAllByDateAsync(new DateOnly(2024, 05, 10));

            list.Should().HaveCount(2);
            list.Select(f => f.FromCurrency.Code)
                .Should().ContainInOrder("EUR", "USD");
        }
    }
}
