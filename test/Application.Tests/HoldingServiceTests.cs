using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PM.Application.Interfaces;
using PM.Application.Services;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Utils.Tests;
using PM.SharedKernel;
using Xunit;

namespace PM.Application.Services.Tests
{
    public class HoldingServiceTests
    {
        private readonly Mock<IAccountRepository> _accountRepoMock;
        private readonly Mock<IHoldingRepository> _holdingRepoMock;
        private readonly HoldingService _service;
        private readonly CancellationToken _ct = CancellationToken.None;

        public HoldingServiceTests()
        {
            _accountRepoMock = new Mock<IAccountRepository>();
            _holdingRepoMock = new Mock<IHoldingRepository>();
            _service = new HoldingService(_accountRepoMock.Object, _holdingRepoMock.Object);

            TestEntityFactory.ResetIds();
        }

        [Fact]
        public async Task UpsertHoldingAsync_Should_Insert_New_Holding_When_NotExists()
        {
            // Arrange
            var acc = TestEntityFactory.CreateAccount("TFSA", new Currency("CAD"));
            var symbol = new Symbol("VFV.TO", "CAD");
            _accountRepoMock.Setup(r => r.GetByIdWithIncludesAsync(acc.Id, It.IsAny<IncludeOption[]>(), _ct))
                            .ReturnsAsync(acc);

            // Act
            var holding = await _service.UpsertHoldingAsync(acc.Id, symbol.ToAsset(), 100, _ct);

            // Assert
            holding.Should().NotBeNull();
            holding.Asset.Should().Be(symbol);
            holding.Quantity.Should().Be(100);
            acc.Holdings.Should().Contain(holding);

            _accountRepoMock.Verify(r => r.UpdateAsync(acc, _ct), Times.Once);
            _accountRepoMock.Verify(r => r.SaveChangesAsync(_ct), Times.Once);
        }

        [Fact]
        public async Task UpsertHoldingAsync_Should_Update_Existing_Holding_When_Already_Present()
        {
            var acc = TestEntityFactory.CreateAccountWithHolding("RRSP", new Currency("CAD"), new Symbol("VFV.TO", "CAD"), 10);
            var symbol = new Symbol("VFV.TO", "CAD");

            _accountRepoMock.Setup(r => r.GetByIdWithIncludesAsync(acc.Id, It.IsAny<IncludeOption[]>(), _ct))
                            .ReturnsAsync(acc);

            var holding = await _service.UpsertHoldingAsync(acc.Id, symbol.ToAsset(), 20, _ct);

            holding.Quantity.Should().Be(30);
            acc.Holdings.Single().Quantity.Should().Be(30);

            _accountRepoMock.Verify(r => r.UpdateAsync(acc, _ct), Times.Once);
        }

        [Fact]
        public async Task UpsertHoldingAsync_Should_Throw_When_Account_NotFound()
        {
            _accountRepoMock.Setup(r => r.GetByIdWithIncludesAsync(999, It.IsAny<IncludeOption[]>(), _ct))
                            .ReturnsAsync((Account?)null);

            var act = async () => await _service.UpsertHoldingAsync(999, new Symbol("VFV.TO", "CAD").ToAsset(), 10, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Account 999 not found*");
        }

        [Fact]
        public async Task UpdateHoldingQuantityAsync_Should_Update_Existing_Holding()
        {
            var acc = TestEntityFactory.CreateAccountWithHolding("RRSP", new Currency("CAD"), new Symbol("VFV.TO", "CAD"), 5);
            _accountRepoMock.Setup(r => r.GetByIdAsync(acc.Id, _ct)).ReturnsAsync(acc);

            var updated = await _service.UpdateHoldingQuantityAsync(acc.Id, new Symbol("VFV.TO", "CAD").ToAsset(), 10, _ct);

            updated.Quantity.Should().Be(10);
            acc.Holdings.Single().Quantity.Should().Be(10);

            _accountRepoMock.Verify(r => r.UpdateAsync(acc, _ct), Times.Once);
        }

        [Fact]
        public async Task UpdateHoldingQuantityAsync_Should_Throw_When_Negative()
        {
            var acc = TestEntityFactory.CreateAccountWithHolding("RRSP", new Currency("CAD"), new Symbol("VFV.TO", "CAD"), 5);

            _accountRepoMock.Setup(r => r.GetByIdAsync(acc.Id, _ct)).ReturnsAsync(acc);

            var act = async () => await _service.UpdateHoldingQuantityAsync(acc.Id, new Symbol("VFV.TO", "CAD").ToAsset(), -5, _ct);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*must be positive*");
        }

        [Fact]
        public async Task RemoveHoldingAsync_Should_Remove_When_Holding_Exists()
        {
            var acc = TestEntityFactory.CreateAccountWithHolding("TFSA", new Currency("CAD"), new Symbol("VFV.TO", "CAD"), 50);
            _accountRepoMock.Setup(r => r.GetByIdAsync(acc.Id, _ct)).ReturnsAsync(acc);

            await _service.RemoveHoldingAsync(acc.Id, new Symbol("VFV.TO", "CAD").ToAsset(), _ct);

            acc.Holdings.Should().BeEmpty();
            _accountRepoMock.Verify(r => r.UpdateAsync(acc, _ct), Times.Once);
            _accountRepoMock.Verify(r => r.SaveChangesAsync(_ct), Times.Once);
        }

        [Fact]
        public async Task RemoveHoldingAsync_Should_Not_Fail_When_Holding_NotFound()
        {
            var acc = TestEntityFactory.CreateAccount("TFSA", new Currency("CAD"));
            _accountRepoMock.Setup(r => r.GetByIdAsync(acc.Id, _ct)).ReturnsAsync(acc);

            await _service.RemoveHoldingAsync(acc.Id, new Symbol("VFV.TO", "CAD").ToAsset(), _ct);

            acc.Holdings.Should().BeEmpty();
            _accountRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Account>(), _ct), Times.Never);
        }

        [Fact]
        public async Task GetHoldingAsync_Should_Return_Holding_When_Exists()
        {
            var acc = TestEntityFactory.CreateAccount("RRSP", new Currency("CAD"));
            var h = TestEntityFactory.CreateHolding(new Symbol("VFV.TO", "CAD"), 100);
            var holdings = new List<Holding> { h };

            _holdingRepoMock.Setup(r => r.ListByAccountAsync(acc.Id, _ct)).ReturnsAsync(holdings);

            var result = await _service.GetHoldingAsync(acc.Id, (Asset)h.Asset, _ct);

            result.Should().BeEquivalentTo(h);
        }

        [Fact]
        public async Task GetCashBalanceAsync_Should_Return_Correct_Quantity()
        {
            var acc = TestEntityFactory.CreateAccountWithHolding("Chequing", new Currency("CAD"), new Symbol("CAD"), 1000);
            _accountRepoMock.Setup(r => r.GetByIdAsync(acc.Id, _ct)).ReturnsAsync(acc);

            var result = await _service.GetCashBalanceAsync(acc.Id, new Currency("CAD"), _ct);

            result.Should().Be(1000);
        }

        [Fact]
        public async Task AddTagAsync_Should_Associate_Tag_With_Holding()
        {
            var acc = TestEntityFactory.CreateAccountWithHolding("RRSP", new Currency("CAD"), new Symbol("VFV.TO", "CAD"), 100);
            var tag = TestEntityFactory.CreateTag("Index");

            _accountRepoMock.Setup(r => r.GetByIdAsync(acc.Id, _ct)).ReturnsAsync(acc);

            await _service.AddTagAsync(acc.Id, new Symbol("VFV.TO", "CAD").ToAsset(), tag, _ct);

            acc.Holdings.Single().Tags.Should().Contain(tag);
        }

        [Fact]
        public async Task RemoveTagAsync_Should_Remove_Tag_When_Exists()
        {
            var acc = TestEntityFactory.CreateAccountWithHolding("RRSP", new Currency("CAD"), new Symbol("VFV.TO", "CAD"), 100);
            var tag = TestEntityFactory.CreateTag("Dividend");
            acc.Holdings.Single().AddTag(tag);

            _accountRepoMock.Setup(r => r.GetByIdAsync(acc.Id, _ct)).ReturnsAsync(acc);

            await _service.RemoveTagAsync(acc.Id, new Symbol("VFV.TO", "CAD").ToAsset(), tag, _ct);

            acc.Holdings.Single().Tags.Should().BeEmpty();
        }
    }
}
