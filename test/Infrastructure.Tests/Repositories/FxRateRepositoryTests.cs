using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PM.Domain.Values;
using PM.Infrastructure.Data;
using Xunit;

namespace PM.Infrastructure.Repositories.Tests
{
    public class FxRateRepositoryTests : IAsyncLifetime
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ValuationDbContext> _options;

        public FxRateRepositoryTests()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _options = new DbContextOptionsBuilder<ValuationDbContext>()
                .UseSqlite(_connection)
                .Options;
        }

        public async Task InitializeAsync()
        {
            await _connection.OpenAsync();
            await using var context = new ValuationDbContext(_options);
            await context.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await _connection.DisposeAsync();
        }

        [Fact]
        public async Task SaveAsync_AddsRateToDatabase()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new FxRateRepository(context);

            var rate = new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10), 1.35m);

            await repo.SaveAsync(rate);

            var saved = await context.FxRates.FirstOrDefaultAsync();
            saved.Should().NotBeNull();
            saved!.Rate.Should().Be(1.35m);
            saved.FromCurrency.Should().Be(Currency.USD);
            saved.ToCurrency.Should().Be(Currency.CAD);
        }

        [Fact]
        public async Task GetAsync_ReturnsExistingRate()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new FxRateRepository(context);
            var rate = new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10), 1.35m);
            context.FxRates.Add(rate);
            await context.SaveChangesAsync();

            var found = await repo.GetAsync(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10));

            found.Should().NotBeNull();
            found!.Rate.Should().Be(1.35m);
        }

        [Fact]
        public async Task GetAsync_ReturnsNull_WhenNotFound()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new FxRateRepository(context);

            var result = await repo.GetAsync(Currency.EUR, Currency.CAD, new DateOnly(2024, 05, 10));

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpsertAsync_UpdatesExistingRate()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new FxRateRepository(context);
            var rate = new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10), 1.35m);
            context.FxRates.Add(rate);
            await context.SaveChangesAsync();

            var updated = new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10), 1.40m);
            await repo.UpsertAsync(updated);

            var saved = await context.FxRates.FirstAsync();
            saved.Rate.Should().Be(1.40m);
        }

        [Fact]
        public async Task UpsertAsync_InsertsNewRate_WhenNotExists()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new FxRateRepository(context);

            var newRate = new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 11), 1.38m);
            await repo.UpsertAsync(newRate);

            var all = await context.FxRates.ToListAsync();
            all.Should().HaveCount(1);
            all[0].Rate.Should().Be(1.38m);
        }

        [Fact]
        public async Task GetAllForPairAsync_ReturnsOrderedRates()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new FxRateRepository(context);

            var rates = new[]
            {
                new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 12), 1.40m),
                new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10), 1.35m),
                new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 11), 1.38m),
            };
            context.FxRates.AddRange(rates);
            await context.SaveChangesAsync();

            var list = await repo.GetAllForPairAsync(Currency.USD, Currency.CAD);

            list.Should().HaveCount(3);
            list.Should().BeInAscendingOrder(f => f.Date);
        }

        [Fact]
        public async Task DeleteAsync_RemovesExistingRate()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new FxRateRepository(context);


            var rate = new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10), 1.35m);
            context.FxRates.Add(rate);
            await context.SaveChangesAsync();

            var deleted = await repo.DeleteAsync(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10));

            deleted.Should().BeTrue();
            (await context.FxRates.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new FxRateRepository(context);

            var deleted = await repo.DeleteAsync(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10));

            deleted.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllByDateAsync_ReturnsOrderedByCurrencyCodes()
        {
            await using var context = new ValuationDbContext(_options);
            var repo = new FxRateRepository(context);

            context.FxRates.AddRange(new[]
            {
                new FxRate(Currency.EUR, Currency.CAD, new DateOnly(2024, 05, 10), 1.48m),
                new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10), 1.35m),
            });
            await context.SaveChangesAsync();

            var list = await repo.GetAllByDateAsync(new DateOnly(2024, 05, 10));

            list.Should().HaveCount(2);
            list.Select(f => f.FromCurrency.Code)
                .Should().ContainInOrder("EUR", "USD");
        }
    }
}
