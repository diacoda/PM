using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.Infrastructure.Data;
using Xunit;

namespace PM.Infrastructure.Repositories.Tests
{
    public class CashFlowRepositoryTests : IAsyncLifetime
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<CashFlowDbContext> _options;

        public CashFlowRepositoryTests()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _options = new DbContextOptionsBuilder<CashFlowDbContext>()
                .UseSqlite(_connection)
                .Options;
        }

        public async Task InitializeAsync()
        {
            await _connection.OpenAsync();
            await using var context = new CashFlowDbContext(_options);
            await context.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await _connection.DisposeAsync();
        }

        private static CashFlow CreateCashFlow(
            int accountId,
            DateOnly date,
            decimal amount,
            Currency currency,
            CashFlowType type)
        {
            return new CashFlow()
            {
                AccountId = accountId,
                Date = date,
                Amount = new Money(amount, currency),
                Type = type,
                Note = "Test flow"
            };
        }

        // -----------------------------------------------------
        // TESTS
        // -----------------------------------------------------

        [Fact]
        public async Task RecordCashFlowAsync_Should_Add_Record_To_Db()
        {
            await using var context = new CashFlowDbContext(_options);
            var repo = new CashFlowRepository(context);

            var flow = CreateCashFlow(1, new DateOnly(2025, 1, 1), 100m, Currency.CAD, CashFlowType.Deposit);

            await repo.RecordCashFlowAsync(flow);

            var fromDb = await context.CashFlows.FirstOrDefaultAsync();
            fromDb.Should().NotBeNull();
            fromDb!.AccountId.Should().Be(1);
            fromDb.Amount.Should().NotBeNull();
            fromDb.Amount.Amount.Should().Be(100m);
            fromDb.Amount.Currency.Should().Be(Currency.CAD);
            fromDb.Amount.Should().Be(new Money(100m, Currency.CAD));
            fromDb.Type.Should().Be(CashFlowType.Deposit);
        }

        [Fact]
        public async Task GetCashFlowsAsync_Should_Return_All_For_Account()
        {
            await using var context = new CashFlowDbContext(_options);
            var repo = new CashFlowRepository(context);

            var flows = new[]
            {
                CreateCashFlow(1, new DateOnly(2025, 1, 1), 100, Currency.CAD, CashFlowType.Deposit),
                CreateCashFlow(1, new DateOnly(2025, 1, 10), 50, Currency.CAD, CashFlowType.Withdrawal),
                CreateCashFlow(2, new DateOnly(2025, 1, 5), 999, Currency.CAD, CashFlowType.Deposit) // different account
            };
            await context.CashFlows.AddRangeAsync(flows);
            await context.SaveChangesAsync();

            var result = await repo.GetCashFlowsAsync(1);

            result.Should().HaveCount(2);
            result.Select(f => f.Date).Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task GetCashFlowsAsync_Should_Filter_By_Date_Range()
        {
            await using var context = new CashFlowDbContext(_options);
            var repo = new CashFlowRepository(context);

            var flows = new[]
            {
                CreateCashFlow(1, new DateOnly(2025, 1, 1), 100, Currency.CAD, CashFlowType.Deposit),
                CreateCashFlow(1, new DateOnly(2025, 1, 10), 200, Currency.CAD, CashFlowType.Deposit),
                CreateCashFlow(1, new DateOnly(2025, 1, 20), 300, Currency.CAD, CashFlowType.Deposit)
            };
            await context.CashFlows.AddRangeAsync(flows);
            await context.SaveChangesAsync();

            var from = new DateOnly(2025, 1, 5);
            var to = new DateOnly(2025, 1, 15);

            var result = await repo.GetCashFlowsAsync(1, from, to);

            result.Should().HaveCount(1);
            result.First().Date.Should().Be(new DateOnly(2025, 1, 10));
        }

        [Theory]
        [InlineData(CashFlowType.Deposit, 100)]
        [InlineData(CashFlowType.Withdrawal, -100)]
        [InlineData(CashFlowType.Fee, -100)]
        [InlineData(CashFlowType.Dividend, 100)]
        [InlineData(CashFlowType.Interest, 100)]
        public async Task GetNetCashFlowAsync_Should_Respect_Type_Signs(
            CashFlowType type, decimal expected)
        {
            await using var context = new CashFlowDbContext(_options);
            var repo = new CashFlowRepository(context);

            var flow = CreateCashFlow(1, new DateOnly(2025, 1, 1), 100, Currency.CAD, type);
            await repo.RecordCashFlowAsync(flow);

            var net = await repo.GetNetCashFlowAsync(1, Currency.CAD);

            net.Amount.Should().Be(expected);
            net.Currency.Should().Be(Currency.CAD);
        }

        [Fact]
        public async Task GetNetCashFlowAsync_Should_Sum_Multiple_Flows_Correctly()
        {
            await using var context = new CashFlowDbContext(_options);
            var repo = new CashFlowRepository(context);

            var flows = new[]
            {
                CreateCashFlow(1, new DateOnly(2025, 1, 1), 100, Currency.CAD, CashFlowType.Deposit),
                CreateCashFlow(1, new DateOnly(2025, 1, 2), 50, Currency.CAD, CashFlowType.Withdrawal),
                CreateCashFlow(1, new DateOnly(2025, 1, 3), 10, Currency.CAD, CashFlowType.Fee)
            };
            await context.CashFlows.AddRangeAsync(flows);
            await context.SaveChangesAsync();

            var net = await repo.GetNetCashFlowAsync(1, Currency.CAD);

            net.Should().Be(new Money(40m, Currency.CAD)); // 100 - 50 - 10
        }

        [Fact]
        public async Task GetNetCashFlowAsync_Should_Filter_By_Currency()
        {
            await using var context = new CashFlowDbContext(_options);
            var repo = new CashFlowRepository(context);

            var flows = new[]
            {
                CreateCashFlow(1, new DateOnly(2025, 1, 1), 100, Currency.CAD, CashFlowType.Deposit),
                CreateCashFlow(1, new DateOnly(2025, 1, 2), 200, Currency.USD, CashFlowType.Deposit)
            };
            await context.CashFlows.AddRangeAsync(flows);
            await context.SaveChangesAsync();

            var netCad = await repo.GetNetCashFlowAsync(1, Currency.CAD);
            var netUsd = await repo.GetNetCashFlowAsync(1, Currency.USD);

            netCad.Should().Be(new Money(100m, Currency.CAD));
            netUsd.Should().Be(new Money(200m, Currency.USD));
        }

        [Fact]
        public async Task GetNetCashFlowAsync_Should_Respect_DateRange()
        {
            await using var context = new CashFlowDbContext(_options);
            var repo = new CashFlowRepository(context);

            var flows = new[]
            {
                CreateCashFlow(1, new DateOnly(2025, 1, 1), 10, Currency.CAD, CashFlowType.Deposit),
                CreateCashFlow(1, new DateOnly(2025, 1, 15), 20, Currency.CAD, CashFlowType.Deposit),
                CreateCashFlow(1, new DateOnly(2025, 1, 30), 30, Currency.CAD, CashFlowType.Deposit)
            };
            await context.CashFlows.AddRangeAsync(flows);
            await context.SaveChangesAsync();

            var from = new DateOnly(2025, 1, 10);
            var to = new DateOnly(2025, 1, 25);

            var net = await repo.GetNetCashFlowAsync(1, Currency.CAD, from, to);

            net.Should().Be(new Money(20m, Currency.CAD));
        }

        [Fact]
        public async Task GetCashFlowByIdAsync_Should_Return_Correct_CashFlow()
        {
            await using var context = new CashFlowDbContext(_options);
            var repo = new CashFlowRepository(context);

            var flow = CreateCashFlow(1, new DateOnly(2025, 2, 1), 123m, Currency.CAD, CashFlowType.Deposit);
            await context.CashFlows.AddAsync(flow);
            await context.SaveChangesAsync();

            var result = await repo.GetCashFlowByIdAsync(flow.Id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(flow.Id);
            result.AccountId.Should().Be(flow.AccountId);
            result.Amount.Should().Be(flow.Amount);
        }

        [Fact]
        public async Task GetCashFlowByIdAsync_Should_Return_Null_If_NotFound()
        {
            await using var context = new CashFlowDbContext(_options);
            var repo = new CashFlowRepository(context);

            var result = await repo.GetCashFlowByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteCashFlowAsync_Should_Remove_Record_From_Db()
        {
            await using var context = new CashFlowDbContext(_options);
            var repo = new CashFlowRepository(context);

            var flow = CreateCashFlow(1, new DateOnly(2025, 3, 1), 250m, Currency.CAD, CashFlowType.Deposit);
            await context.CashFlows.AddAsync(flow);
            await context.SaveChangesAsync();

            // Act
            await repo.DeleteCashFlowAsync(flow);

            // Assert
            var exists = await context.CashFlows.AnyAsync(f => f.Id == flow.Id);
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteCashFlowAsync_Should_Throw_If_SaveChanges_Returns_Zero()
        {
            // Arrange
            var mockSet = new Mock<DbSet<CashFlow>>();
            var mockContext = new Mock<CashFlowDbContext>(_options);

            mockContext
                .Setup(c => c.Set<CashFlow>())
                .Returns(mockSet.Object);

            // Simulate SaveChangesAsync returning 0
            mockContext
                .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var repo = new CashFlowRepository(mockContext.Object);

            var flow = CreateCashFlow(1, new DateOnly(2025, 3, 1), 100m, Currency.CAD, CashFlowType.Deposit);

            // Act
            Func<Task> act = async () => await repo.DeleteCashFlowAsync(flow);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Failed to delete cash flow.");

            // Ensure Remove was called
            mockSet.Verify(s => s.Remove(It.Is<CashFlow>(f => f == flow)), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


    }
}
