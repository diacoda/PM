using System;
using System.Collections.Generic;
using System.Linq;
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
    public class CashFlowServiceTests
    {
        private readonly Mock<ICashFlowRepository> _repoMock;
        private readonly CashFlowService _service;

        public CashFlowServiceTests()
        {
            _repoMock = new Mock<ICashFlowRepository>();
            _service = new CashFlowService(_repoMock.Object);
        }

        [Fact]
        public async Task RecordCashFlowAsync_Should_Call_Repository_With_Correct_CashFlow()
        {
            // Arrange
            var accountId = 1;
            var date = new DateOnly(2025, 1, 1);
            var amount = new Money(100m, Currency.CAD);
            var type = CashFlowType.Deposit;
            var note = "Test Deposit";

            var expectedFlow = new CashFlow
            {
                AccountId = accountId,
                Date = date,
                Amount = amount,
                Type = type,
                Note = note,
                //Id = 123
            };

            _repoMock
                .Setup(r => r.RecordCashFlowAsync(It.IsAny<CashFlow>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedFlow)
                .Verifiable();

            // Act
            var result = await _service.RecordCashFlowAsync(accountId, date, amount, type, note);

            // Assert
            result.Should().NotBeNull();
            //result.Id.Should().Be(expectedFlow.Id);
            result.Type.Should().Be(type);
            result.Amount.Amount.Should().Be(amount.Amount);

            _repoMock.Verify(r => r.RecordCashFlowAsync(
                It.Is<CashFlow>(cf =>
                    cf.AccountId == accountId &&
                    cf.Date == date &&
                    cf.Amount.Amount == amount.Amount &&
                    cf.Amount.Currency.Code == amount.Currency.Code &&
                    cf.Type == type &&
                    cf.Note == note),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }


        [Fact]
        public async Task GetCashFlowsAsync_Should_Return_Repository_Data()
        {
            // Arrange
            var account = TestEntityFactory.CreateAccount("TestAccount", Currency.CAD);
            var flows = new List<CashFlow>
            {
                new CashFlow { AccountId = account.Id, Amount = new Money(100, Currency.CAD), Date = new DateOnly(2025,1,1), Type = CashFlowType.Deposit },
                new CashFlow { AccountId = account.Id, Amount = new Money(50, Currency.CAD), Date = new DateOnly(2025,1,2), Type = CashFlowType.Withdrawal }
            };

            _repoMock.Setup(r => r.GetCashFlowsAsync(account.Id, null, null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(flows);

            // Act
            var result = await _service.GetCashFlowsAsync(account);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainSingle(cf => cf.Amount.Amount == 100 && cf.Type == CashFlowType.Deposit);
            result.Should().ContainSingle(cf => cf.Amount.Amount == 50 && cf.Type == CashFlowType.Withdrawal);
        }

        [Fact]
        public async Task GetNetCashFlowAsync_Should_Return_Repository_Result()
        {
            // Arrange
            var account = TestEntityFactory.CreateAccount("TestAccount", Currency.CAD);
            var currency = Currency.CAD;
            var net = new Money(150, currency);

            _repoMock.Setup(r => r.GetNetCashFlowAsync(account.Id, currency, null, null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(net);

            // Act
            var result = await _service.GetNetCashFlowAsync(account, currency);

            // Assert
            result.Amount.Should().Be(150);
            result.Currency.Should().Be(currency);
        }

        [Fact]
        public async Task GetPortfolioNetCashFlowAsync_Should_Return_Sum_Of_Accounts()
        {
            // Arrange
            var currency = Currency.CAD;

            var account1 = TestEntityFactory.CreateAccount("Account1", currency);
            var account2 = TestEntityFactory.CreateAccount("Account2", currency);

            var portfolio = new Portfolio("MyPortfolio");
            portfolio.AddAccount(account1);
            portfolio.AddAccount(account2);

            var net1 = new Money(100, currency);
            var net2 = new Money(50, currency);

            _repoMock.Setup(r => r.GetNetCashFlowAsync(account1.Id, currency, null, null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(net1);
            _repoMock.Setup(r => r.GetNetCashFlowAsync(account2.Id, currency, null, null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(net2);

            // Act
            var result = await _service.GetPortfolioNetCashFlowAsync(portfolio, currency);

            // Assert
            result.Amount.Should().Be(150);
            result.Currency.Should().Be(currency);
        }

        [Fact]
        public async Task RecordCashFlowAsync_Should_Throw_When_Repository_Throws()
        {
            // Arrange
            _repoMock.Setup(r => r.RecordCashFlowAsync(It.IsAny<CashFlow>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new InvalidOperationException("Repo error"));

            // Act
            Func<Task> act = () => _service.RecordCashFlowAsync(1, new DateOnly(2025, 1, 1), new Money(100, Currency.CAD), CashFlowType.Deposit);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Repo error");
        }

        [Fact]
        public async Task GetCashFlowByIdAsync_Should_Return_CashFlow_From_Repository()
        {
            // Arrange
            var flow = TestEntityFactory.CreateCashFlow(1, new Money(100m, Currency.CAD), CashFlowType.Deposit, "Note");

            _repoMock
                .Setup(r => r.GetCashFlowByIdAsync(flow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(flow);

            // Act
            var result = await _service.GetCashFlowByIdAsync(flow.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(flow.Id);
            result.Amount.Amount.Should().Be(flow.Amount.Amount);
            result.Type.Should().Be(flow.Type);

            _repoMock.Verify(r => r.GetCashFlowByIdAsync(flow.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetCashFlowByIdAsync_Should_Return_Null_If_NotFound()
        {
            // Arrange
            var cashFlowId = 999;
            _repoMock
                .Setup(r => r.GetCashFlowByIdAsync(cashFlowId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CashFlow?)null);

            // Act
            var result = await _service.GetCashFlowByIdAsync(cashFlowId);

            // Assert
            result.Should().BeNull();
            _repoMock.Verify(r => r.GetCashFlowByIdAsync(cashFlowId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCashFlowAsync_Should_Call_Repository_Delete_When_Found()
        {
            // Arrange
            var flow = TestEntityFactory.CreateCashFlow(1, new Money(100m, Currency.CAD), CashFlowType.Deposit, "Note");

            _repoMock
                .Setup(r => r.GetCashFlowByIdAsync(flow.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(flow);

            _repoMock
                .Setup(r => r.DeleteCashFlowAsync(flow, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _service.DeleteCashFlowAsync(flow.Id);

            // Assert
            _repoMock.Verify(r => r.GetCashFlowByIdAsync(flow.Id, It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.DeleteCashFlowAsync(flow, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCashFlowAsync_Should_Throw_When_CashFlow_NotFound()
        {
            // Arrange
            var cashFlowId = 999;
            _repoMock
                .Setup(r => r.GetCashFlowByIdAsync(cashFlowId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CashFlow?)null);

            // Act
            Func<Task> act = () => _service.DeleteCashFlowAsync(cashFlowId);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Cash flow not found.");
            _repoMock.Verify(r => r.GetCashFlowByIdAsync(cashFlowId, It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.DeleteCashFlowAsync(It.IsAny<CashFlow>(), It.IsAny<CancellationToken>()), Times.Never);
        }

    }
}
