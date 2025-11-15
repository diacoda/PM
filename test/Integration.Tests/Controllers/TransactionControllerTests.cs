using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PM.API.Controllers;
using PM.Application.Interfaces;
using PM.DTO;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.Utils.Tests;
using PM.Domain.Mappers;

namespace PM.Integration.Controllers.Tests
{
    public class TransactionsControllerTests
    {
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly Mock<ITransactionWorkflowService> _workflowMock;
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly TransactionsController _controller;

        public TransactionsControllerTests()
        {
            _transactionServiceMock = new Mock<ITransactionService>();
            _workflowMock = new Mock<ITransactionWorkflowService>();
            _accountServiceMock = new Mock<IAccountService>();

            _controller = new TransactionsController(
                _transactionServiceMock.Object,
                _workflowMock.Object,
                _accountServiceMock.Object
            );
        }

        private static CashFlowDTO CreateCashFlowDto(decimal amount = 1000m, string currency = "CAD") =>
            new(amount, currency, DateOnly.FromDateTime(DateTime.UtcNow), "note");

        private static CreateTransactionDTO CreateTransactionDto() =>
            new() { Type = TransactionType.Buy.ToString(), Symbol = "VFV.TO", Quantity = 10, Amount = 1000m, Date = DateOnly.FromDateTime(DateTime.UtcNow) };

        [Fact]
        public async Task Deposit_ReturnsBadRequest_WhenAccountInvalid()
        {
            _accountServiceMock
                .Setup(a => a.GetAccountAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AccountDTO?)null);

            var result = await _controller.Deposit(1, 1, CreateCashFlowDto(), CancellationToken.None);

            var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            ((ProblemDetails)badRequest.Value!).Title.Should().Be("Invalid portfolio/account");
        }

        [Fact]
        public async Task Deposit_ReturnsOk_WhenAccountValid()
        {
            var account = TestEntityFactory.CreateAccount("RRSP", new Currency("CAD"));
            var accountDto = AccountMapper.ToDTO(account);

            _accountServiceMock
                .Setup(a => a.GetAccountAsync(account.PortfolioId, account.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountDto);

            var tx = TestEntityFactory.CreateTransaction(
                account.Id,
                TransactionType.Deposit,
                new Symbol("CAD"),
                0m,
                new Money(1000m, new Currency("CAD")),
                DateOnly.FromDateTime(DateTime.UtcNow));

            var txDto = TransactionMapper.ToDTO(tx);
            txDto.Type = "Deposit";

            _workflowMock
                .Setup(w => w.ProcessTransactionAsync(account.PortfolioId, It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(txDto);

            var result = await _controller.Deposit(account.PortfolioId, account.Id, CreateCashFlowDto(), CancellationToken.None);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeAssignableTo<TransactionDTO>().Subject;
            dto.Id.Should().Be(tx.Id);
            dto.Type.Should().Be("Deposit");
        }


        [Fact]
        public async Task Withdraw_ReturnsOk_WhenAccountValid()
        {
            var account = TestEntityFactory.CreateAccount("RRSP", new Currency("CAD"));
            var accountDto = AccountMapper.ToDTO(account);

            _accountServiceMock
                .Setup(a => a.GetAccountAsync(account.PortfolioId, account.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountDto);

            var tx = TestEntityFactory.CreateTransaction(
                account.Id,
                TransactionType.Withdrawal,
                new Symbol("CAD"),
                0m,
                new Money(500m, new Currency("CAD")),
                DateOnly.FromDateTime(DateTime.UtcNow));

            var txDto = TransactionMapper.ToDTO(tx);
            txDto.Type = "Withdrawal";

            _workflowMock
                .Setup(w => w.ProcessTransactionAsync(account.PortfolioId, It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(txDto);

            var result = await _controller.Withdraw(account.PortfolioId, account.Id, CreateCashFlowDto(500m), CancellationToken.None);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeAssignableTo<TransactionDTO>().Subject;
            dto.Amount.Should().Be(500m);
            dto.Type.Should().Be("Withdrawal");
        }


        [Fact]
        public async Task CreateTransaction_ReturnsOk_WhenAccountValid()
        {
            var account = TestEntityFactory.CreateAccount("RRSP", new Currency("CAD"));
            var accountDto = AccountMapper.ToDTO(account);

            _accountServiceMock
                .Setup(a => a.GetAccountAsync(account.PortfolioId, account.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountDto);

            var tx = TestEntityFactory.CreateTransaction(
                account.Id,
                TransactionType.Buy,
                new Symbol("VFV.TO"),
                10m,
                new Money(1000m, new Currency("CAD")),
                DateOnly.FromDateTime(DateTime.UtcNow));

            var txDto = TransactionMapper.ToDTO(tx);
            txDto.Type = "Buy";

            _workflowMock
                .Setup(w => w.ProcessTransactionAsync(account.PortfolioId, It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(txDto);

            var createDto = TransactionMapper.ToCreateDTO(tx);
            var result = await _controller.CreateTransaction(account.PortfolioId, account.Id, createDto, CancellationToken.None);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeAssignableTo<TransactionDTO>().Subject;
            dto.Id.Should().Be(tx.Id);
            dto.Type.Should().Be("Buy");
        }


        [Fact]
        public async Task GetList_ReturnsOk_WithTransactions()
        {
            var account = TestEntityFactory.CreateAccount("RRSP", new Currency("CAD"));
            var accountDto = AccountMapper.ToDTO(account);

            _accountServiceMock
                .Setup(a => a.GetAccountAsync(account.PortfolioId, account.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountDto);

            var transactions = new List<Transaction>
            {
                TestEntityFactory.CreateTransaction(account.Id, TransactionType.Deposit, new Symbol("CAD"), 0, new Money(1000m, new Currency("CAD")), DateOnly.FromDateTime(DateTime.UtcNow)),
                TestEntityFactory.CreateTransaction(account.Id, TransactionType.Withdrawal, new Symbol("CAD"), 0, new Money(500m, new Currency("CAD")), DateOnly.FromDateTime(DateTime.UtcNow))
            };

            _transactionServiceMock
                .Setup(s => s.ListTransactionsAsync(account.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);

            var result = await _controller.GetList(account.PortfolioId, account.Id, CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dtoList = okResult.Value.Should().BeAssignableTo<IEnumerable<TransactionDTO>>().Subject;
            dtoList.Should().HaveCount(2);
        }

        [Fact]
        public async Task Get_ReturnsOk_WhenTransactionExists()
        {
            var account = TestEntityFactory.CreateAccount("RRSP", new Currency("CAD"));
            var accountDto = AccountMapper.ToDTO(account);

            _accountServiceMock
                .Setup(a => a.GetAccountAsync(account.PortfolioId, account.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountDto);

            var tx = TestEntityFactory.CreateTransaction(account.Id, TransactionType.Buy, new Symbol("VFV.TO"), 10, new Money(1000m, new Currency("CAD")), DateOnly.FromDateTime(DateTime.UtcNow));
            _transactionServiceMock
                .Setup(s => s.GetTransactionAsync(account.Id, tx.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tx);

            var result = await _controller.Get(account.PortfolioId, account.Id, tx.Id, CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var dto = okResult.Value.Should().BeAssignableTo<TransactionDTO>().Subject;
            dto.Id.Should().Be(tx.Id);
            dto.Type.Should().Be("Buy");
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenTransactionDoesNotExist()
        {
            var account = TestEntityFactory.CreateAccount("RRSP", new Currency("CAD"));
            var accountDto = AccountMapper.ToDTO(account);

            _accountServiceMock
                .Setup(a => a.GetAccountAsync(account.PortfolioId, account.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountDto);

            _transactionServiceMock
                .Setup(s => s.GetTransactionAsync(account.Id, 999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Transaction?)null);

            var result = await _controller.Get(account.PortfolioId, account.Id, 999, CancellationToken.None);

            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            ((ProblemDetails)notFound.Value!).Title.Should().Be("Transaction not found");
        }

        [Fact]
        public async Task Deposit_ReturnsOk_CallsWorkflow()
        {
            var account = TestEntityFactory.CreateAccount("RRSP", new Currency("CAD"));
            var accountDto = AccountMapper.ToDTO(account);

            _accountServiceMock
                .Setup(a => a.GetAccountAsync(account.PortfolioId, account.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountDto);

            var tx = TestEntityFactory.CreateTransaction(
                account.Id,
                TransactionType.Deposit,
                new Symbol("CAD"),
                0,
                new Money(1000m, new Currency("CAD")),
                DateOnly.FromDateTime(DateTime.UtcNow));

            var txDto = TransactionMapper.ToDTO(tx);

            _workflowMock
                .Setup(w => w.ProcessTransactionAsync(account.PortfolioId, It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(txDto);

            await _controller.Deposit(account.PortfolioId, account.Id, CreateCashFlowDto(), CancellationToken.None);

            _workflowMock.Verify(w =>
                w.ProcessTransactionAsync(account.PortfolioId,
                    It.Is<Transaction>(t => t.AccountId == account.Id && t.Type == TransactionType.Deposit),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

    }
}
