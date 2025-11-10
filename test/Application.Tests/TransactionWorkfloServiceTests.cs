using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PM.Application.Interfaces;
using PM.Application.Services;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.Utils.Tests;
using Xunit;

namespace PM.Application.Services.Tests
{
    public class TransactionWorkflowServiceTests
    {
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly Mock<ICashFlowService> _cashFlowServiceMock;
        private readonly Mock<IHoldingService> _holdingServiceMock;

        private readonly TransactionWorkflowService _sut;

        public TransactionWorkflowServiceTests()
        {
            // Strict mocks ensure we only allow expected calls.
            _transactionServiceMock = new Mock<ITransactionService>(MockBehavior.Strict);
            _cashFlowServiceMock = new Mock<ICashFlowService>(MockBehavior.Strict);
            _holdingServiceMock = new Mock<IHoldingService>(MockBehavior.Strict);

            _sut = new TransactionWorkflowService(
                _transactionServiceMock.Object,
                _cashFlowServiceMock.Object,
                _holdingServiceMock.Object);
        }

        // 🔹 Helper: create a consistent base transaction
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
        [InlineData(TransactionType.Sell, CashFlowType.Sell, 990)]     // amount - cost
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

            // Cash flow: verify the exact parameters are passed
            _cashFlowServiceMock
                .Setup(cf => cf.RecordCashFlowAsync(
                    tx.AccountId,
                    tx.Date,
                    tx.Amount,
                    expectedFlow,
                    "tx.Note",
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Holding service mock returns a Holding reflecting passed-in Asset & quantity
            _holdingServiceMock
                .Setup(h => h.UpsertHoldingAsync(
                    It.IsAny<int>(),
                    It.IsAny<Asset>(),
                    It.IsAny<decimal>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((int accountId, Asset asset, decimal qty, CancellationToken ct)
                    => new Holding(asset, qty));

            // Act
            var result = await _sut.ProcessTransactionAsync(tx);

            // Assert transaction returned
            result.Should().Be(tx);
            _transactionServiceMock.Verify(s => s.CreateAsync(tx, It.IsAny<CancellationToken>()), Times.Once);

            // Assert cash flow recorded with exact parameters
            _cashFlowServiceMock.Verify(cf =>
                cf.RecordCashFlowAsync(
                    tx.AccountId,
                    tx.Date,
                    tx.Amount,
                    expectedFlow,
                    "tx.Note",
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Assert holdings updated correctly
            switch (txType)
            {
                case TransactionType.Deposit:
                    VerifyHolding(tx.AccountId, "CAD", expectedCashDelta);
                    break;

                case TransactionType.Withdrawal:
                    VerifyHolding(tx.AccountId, "CAD", expectedCashDelta);
                    break;

                case TransactionType.Buy:
                    VerifyHolding(tx.AccountId, "VFV.TO", tx.Quantity);              // symbol
                    VerifyHolding(tx.AccountId, "CAD", expectedCashDelta);            // cash
                    break;

                case TransactionType.Sell:
                    VerifyHolding(tx.AccountId, "VFV.TO", -tx.Quantity);             // symbol
                    VerifyHolding(tx.AccountId, "CAD", expectedCashDelta);            // cash
                    break;

                case TransactionType.Dividend:
                    VerifyHolding(tx.AccountId, "CAD", expectedCashDelta);
                    break;
            }

            // Ensure no extra calls occurred
            _holdingServiceMock.VerifyNoOtherCalls();
            _cashFlowServiceMock.VerifyNoOtherCalls();
            _transactionServiceMock.VerifyNoOtherCalls();
        }


        // 🔹 Unsupported transaction types should be handled gracefully
        [Fact]
        public async Task ProcessTransactionAsync_Should_Handle_Unsupported_Type_Gracefully()
        {
            // Arrange
            var tx = CreateBaseTx((TransactionType)999); // unsupported

            _transactionServiceMock
                .Setup(s => s.CreateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tx);

            // Act
            var result = await _sut.ProcessTransactionAsync(tx);

            // Assert
            result.Should().Be(tx);

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
