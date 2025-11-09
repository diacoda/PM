using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PM.Application.Services;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.Application.Interfaces;
using Xunit;
using PM.Utils.Tests;

namespace PM.Application.Services.Tests
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _txRepoMock;
        private readonly Mock<IAccountRepository> _accountRepoMock;
        private readonly TransactionService _service;
        private readonly CancellationToken _ct = CancellationToken.None;

        public TransactionServiceTests()
        {
            _txRepoMock = new Mock<ITransactionRepository>();
            _accountRepoMock = new Mock<IAccountRepository>();
            _service = new TransactionService(_txRepoMock.Object, _accountRepoMock.Object);
        }
        [Fact]
        public async Task CreateAsync_Should_Add_Transaction_And_Save()
        {
            // Arrange
            var tx = new Transaction
            {
                //Id = 1,
                Type = TransactionType.Buy,
                Symbol = new Symbol("VFV.TO", "CAD"),
                Quantity = 100,
                Amount = new Money(1000, new Currency("CAD")),
                Date = new DateOnly(2025, 1, 1)
            };

            Transaction? capturedTx = null;

            _txRepoMock.Setup(r => r.AddAsync(It.IsAny<Transaction>(), _ct))
                       .Callback<Transaction, CancellationToken>((t, _) => capturedTx = t)
                       .Returns(Task.CompletedTask);

            _txRepoMock.Setup(r => r.SaveChangesAsync(_ct)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateAsync(tx, _ct);

            // Assert
            result.Should().Be(tx);
            capturedTx.Should().Be(tx);
            _txRepoMock.Verify(r => r.AddAsync(tx, _ct), Times.Once);
            _txRepoMock.Verify(r => r.SaveChangesAsync(_ct), Times.Once);
        }

        [Fact]
        public async Task ListTransactionsAsync_Should_Return_Transactions_For_Account()
        {
            // Arrange
            var accountId = 1;
            var txs = new List<Transaction>
            {
                new Transaction { Quantity = 10 },
                new Transaction { Quantity = 20 }
            };

            _txRepoMock.Setup(r => r.ListByAccountAsync(accountId, _ct)).ReturnsAsync(txs);

            // Act
            var result = await _service.ListTransactionsAsync(accountId, _ct);

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(txs);
        }

        [Fact]
        public async Task GetTransactionAsync_Should_Return_Transaction_When_Found()
        {
            // Arrange
            var accountId = 1;
            var tx1 = TestEntityFactory.CreateTransaction(accountId, TransactionType.Buy, new Symbol("VFV.TO", "CAD"), 10, new Money(1000, new Currency("CAD")), DateOnly.FromDateTime(DateTime.Today));
            var tx2 = TestEntityFactory.CreateTransaction(accountId, TransactionType.Sell, new Symbol("BTCC.TO", "CAD"), 5, new Money(500, new Currency("CAD")), DateOnly.FromDateTime(DateTime.Today));
            var txs = new List<Transaction>
            {
                tx1,
                tx2
            };
            _txRepoMock.Setup(r => r.ListByAccountAsync(accountId, _ct)).ReturnsAsync(txs);

            // Act
            var result = await _service.GetTransactionAsync(accountId, tx2.Id, _ct);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(tx2.Id);
        }

        [Fact]
        public async Task GetTransactionAsync_Should_Return_Null_When_NotFound()
        {
            var accountId = 1;
            _txRepoMock.Setup(r => r.ListByAccountAsync(accountId, _ct)).ReturnsAsync(new List<Transaction>());

            var result = await _service.GetTransactionAsync(accountId, 99, _ct);

            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteTransactionAsync_Should_Remove_Transaction_And_Save()
        {
            // Arrange
            var account = TestEntityFactory.CreateAccount("RRSP", new Currency("CAD"));
            var tx = TestEntityFactory.CreateTransaction(account.Id, TransactionType.Buy, new Symbol("VFV.TO", "CAD"), 10, new Money(1000, new Currency("CAD")), DateOnly.FromDateTime(DateTime.Today));
            account.AddTransaction(tx);

            _accountRepoMock.Setup(r => r.UpdateAsync(account, _ct)).Returns(Task.CompletedTask);
            _accountRepoMock.Setup(r => r.SaveChangesAsync(_ct)).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteTransactionAsync(account, tx.Id, _ct);

            // Assert
            account.Transactions.Should().BeEmpty();
            _accountRepoMock.Verify(r => r.UpdateAsync(account, _ct), Times.Once);
            _accountRepoMock.Verify(r => r.SaveChangesAsync(_ct), Times.Once);
        }

        [Fact]
        public async Task DeleteTransactionAsync_Should_Do_Nothing_When_Transaction_NotFound()
        {
            // Arrange
            var account = new Account("RRSP", new Currency("CAD"), FinancialInstitutions.TD);

            // Act
            await _service.DeleteTransactionAsync(account, 99, _ct);

            // Assert
            account.Transactions.Should().BeEmpty();
            _accountRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Account>(), _ct), Times.Never);
            _accountRepoMock.Verify(r => r.SaveChangesAsync(_ct), Times.Never);
        }

        [Theory]
        [InlineData("VFV.TO", "CAD", 1000)]
        [InlineData("BTCC.TO", "CAD", 0.5)]
        [InlineData("ZGLD.TO", "CAD", 50)]
        public async Task CreateAsync_Should_Handle_Different_Assets_And_Currencies(string code, string currency, decimal quantity)
        {
            var tx = new Transaction
            {
                Symbol = new Symbol(code, currency),
                Quantity = quantity,
                Amount = new Money(quantity * 10, new Currency(currency)),
                Type = TransactionType.Buy,
                Date = DateOnly.FromDateTime(DateTime.Today),
                AccountId = 1
            };

            _txRepoMock.Setup(r => r.AddAsync(tx, _ct)).Returns(Task.CompletedTask);
            _txRepoMock.Setup(r => r.SaveChangesAsync(_ct)).Returns(Task.CompletedTask);

            var result = await _service.CreateAsync(tx, _ct);

            result.Should().Be(tx);
            result.Quantity.Should().Be(quantity);
            result.Symbol.Code.Should().Be(code);
            result.Symbol.Currency.Code.Should().Be(currency);
        }
    }
}
