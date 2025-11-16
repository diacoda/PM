using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PM.Application.Interfaces;
using PM.Application.Services;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.InMemoryEventBus;
using PM.SharedKernel;
using PM.SharedKernel.Events;
using PM.Utils.Tests;
using Xunit;

namespace PM.Application.Services.Tests
{
    public class TransactionWorkflowServiceTests
    {
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly Mock<ICashFlowService> _cashFlowServiceMock;
        private readonly Mock<IHoldingService> _holdingServiceMock;
        private readonly Mock<PM.SharedKernel.Events.IDomainEventDispatcher> _dispatcherServiceMock;
        private readonly Mock<PM.InMemoryEventBus.IEventDispatcher> _eventDispatcherServiceMock;

        private readonly TransactionWorkflowService _sut;

        public TransactionWorkflowServiceTests()
        {
            // Strict mocks ensure we only allow expected calls.
            _transactionServiceMock = new Mock<ITransactionService>(MockBehavior.Strict);
            _cashFlowServiceMock = new Mock<ICashFlowService>(MockBehavior.Strict);
            _holdingServiceMock = new Mock<IHoldingService>(MockBehavior.Strict);
            _dispatcherServiceMock = new Mock<PM.SharedKernel.Events.IDomainEventDispatcher>(MockBehavior.Strict);
            _eventDispatcherServiceMock = new Mock<PM.InMemoryEventBus.IEventDispatcher>(MockBehavior.Strict);

            _sut = new TransactionWorkflowService(
                _transactionServiceMock.Object,
                _cashFlowServiceMock.Object,
                _holdingServiceMock.Object,
                _dispatcherServiceMock.Object,
                _eventDispatcherServiceMock.Object
                );
        }

        private static Transaction CreateBaseTx(TransactionType type)
        {
            return TestEntityFactory.CreateTransaction(
                100,
                type,
                new Symbol("VFV.TO"),
                10,
                new Money(1000m, new Currency("CAD")),
                new Money(10m, new Currency("CAD"))
            );
        }

        private void VerifyHolding(int accountId, string symbolCode, decimal expectedQty)
        {
            _holdingServiceMock.Verify(h =>
                h.UpsertHoldingAsync(
                    accountId,
                    It.Is<Asset>(a => a.Code == symbolCode),
                    expectedQty,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [InlineData(TransactionType.Deposit, CashFlowType.Deposit, 1000)]
        [InlineData(TransactionType.Withdrawal, CashFlowType.Withdrawal, -1000)]
        [InlineData(TransactionType.Buy, CashFlowType.Buy, -1010)]      // amount + cost
        [InlineData(TransactionType.Sell, CashFlowType.Sell, 990)]      // amount - cost
        [InlineData(TransactionType.Dividend, CashFlowType.Dividend, 990)]
        public async Task ProcessTransactionAsync_Should_Record_Correct_CashFlow_And_Update_Holdings(
            TransactionType txType,
            CashFlowType expectedFlow,
            decimal expectedCashDelta)
        {
            // Arrange
            var tx = CreateBaseTx(txType);

            _transactionServiceMock
                .Setup(s => s.CreateAsync(It.Is<Transaction>(t => t == tx), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tx);

            _dispatcherServiceMock
                .Setup(d => d.DispatchEntityEventsAsync(
                    It.IsAny<Entity>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _eventDispatcherServiceMock
                .Setup(d => d.DispatchEntityEventsAsync(
                    It.IsAny<Entity>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var expectedCashFlow = TestEntityFactory.CreateCashFlow(
                tx.AccountId,
                tx.Amount,
                expectedFlow,
                "tx.Note");

            _cashFlowServiceMock
                .Setup(cf => cf.RecordCashFlowAsync(
                    tx.AccountId,
                    tx.Date,
                    tx.Amount,
                    expectedFlow,
                    "tx.Note",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCashFlow);

            _holdingServiceMock
                .Setup(h => h.UpsertHoldingAsync(
                    It.IsAny<int>(),
                    It.IsAny<Asset>(),
                    It.IsAny<decimal>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((int accountId, Asset asset, decimal qty, CancellationToken ct) =>
                {
                    var holding = TestEntityFactory.CreateHolding(asset.ToSymbol(), qty);
                    return holding;
                });

            // Act
            var result = await _sut.ProcessTransactionAsync(1, tx);

            // Assert
            result.Should().NotBeNull();
            result.CashFlowId.Should().Be(expectedCashFlow.Id);
            result.HoldingIds.Should().NotBeEmpty();

            // Verify: transaction persisted once
            _transactionServiceMock.Verify(
                s => s.CreateAsync(tx, It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify: correct cash flow recorded
            _cashFlowServiceMock.Verify(cf =>
                cf.RecordCashFlowAsync(
                    tx.AccountId,
                    tx.Date,
                    tx.Amount,
                    expectedFlow,
                    "tx.Note",
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify: holdings updated correctly
            switch (txType)
            {
                case TransactionType.Deposit:
                case TransactionType.Withdrawal:
                case TransactionType.Dividend:
                    VerifyHolding(tx.AccountId, "CAD", expectedCashDelta);
                    break;

                case TransactionType.Buy:
                    VerifyHolding(tx.AccountId, "VFV.TO", tx.Quantity);
                    VerifyHolding(tx.AccountId, "CAD", expectedCashDelta);
                    break;

                case TransactionType.Sell:
                    VerifyHolding(tx.AccountId, "VFV.TO", -tx.Quantity);
                    VerifyHolding(tx.AccountId, "CAD", expectedCashDelta);
                    break;
            }

            _eventDispatcherServiceMock.Verify(
                d => d.DispatchEntityEventsAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()),
                Times.Once
            );

            // Verify no unexpected interactions
            _holdingServiceMock.VerifyNoOtherCalls();
            _cashFlowServiceMock.VerifyNoOtherCalls();
            _transactionServiceMock.VerifyNoOtherCalls();
            _eventDispatcherServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ProcessTransactionAsync_Should_Handle_Unsupported_Type_Gracefully()
        {
            // Arrange
            var tx = CreateBaseTx((TransactionType)999); // unsupported type
            tx.Date = new DateOnly(2025, 11, 10);        // make deterministic

            _transactionServiceMock
                .Setup(s => s.CreateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tx);

            _dispatcherServiceMock
                .Setup(d => d.DispatchEntityEventsAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _eventDispatcherServiceMock
                .Setup(d => d.DispatchEntityEventsAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.ProcessTransactionAsync(1, tx);

            // Assert: verify we got a DTO mapped correctly
            result.Should().NotBeNull();
            result.AccountId.Should().Be(tx.AccountId);
            result.Type.Should().Be("999");
            result.Symbol.Should().Be("VFV.TO");
            result.Quantity.Should().Be(10m);
            result.Amount.Should().Be(1000m);
            result.AmountCurrency.Should().Be("CAD");
            result.Costs.Should().Be(10m);
            result.CostsCurrency.Should().Be("CAD");
            result.Date.Should().Be(tx.Date);

            // Unsupported type should not record cash flow or update holdings
            _cashFlowServiceMock.Verify(cf =>
                cf.RecordCashFlowAsync(
                    It.IsAny<int>(),
                    It.IsAny<DateOnly>(),
                    It.IsAny<Money>(),
                    It.IsAny<CashFlowType>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _holdingServiceMock.VerifyNoOtherCalls();
        }
    }
}
