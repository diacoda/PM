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
using PM.DTO;
using PM.SharedKernel;
using PM.Utils.Tests;
using Xunit;

namespace PM.Application.Services.Tests
{
    public class PortfolioServiceTests
    {
        private readonly Mock<IPortfolioRepository> _portfolioRepoMock;
        private readonly Mock<IAccountRepository> _accountRepoMock;
        private readonly PortfolioService _service;
        private readonly CancellationToken _ct = CancellationToken.None;

        public PortfolioServiceTests()
        {
            _portfolioRepoMock = new Mock<IPortfolioRepository>();
            _accountRepoMock = new Mock<IAccountRepository>();
            _service = new PortfolioService(_portfolioRepoMock.Object, _accountRepoMock.Object);
        }

        [Fact]
        public async Task CreateAsync_Should_Create_Portfolio_And_Return_DTO()
        {
            // Arrange
            string owner = "Alice";

            Portfolio? savedPortfolio = null;
            _portfolioRepoMock.Setup(r => r.AddAsync(It.IsAny<Portfolio>(), _ct))
                              .Callback<Portfolio, CancellationToken>((p, ct) => savedPortfolio = p)
                              .Returns(Task.CompletedTask);

            _portfolioRepoMock.Setup(r => r.SaveChangesAsync(_ct)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateAsync(owner, _ct);

            // Assert
            result.Should().NotBeNull();
            result.Owner.Should().Be(owner);
            savedPortfolio.Should().NotBeNull();
            savedPortfolio!.Owner.Should().Be(owner);

            _portfolioRepoMock.Verify(r => r.AddAsync(It.IsAny<Portfolio>(), _ct), Times.Once);
            _portfolioRepoMock.Verify(r => r.SaveChangesAsync(_ct), Times.Once);
        }

        [Fact]
        public async Task UpdateOwnerAsync_Should_Update_Owner_When_Portfolio_Exists()
        {

            var portfolio = TestEntityFactory.CreatePortfolio("OldOwner");
            _portfolioRepoMock.Setup(r => r.GetByIdAsync(portfolio.Id, _ct)).ReturnsAsync(portfolio);
            _portfolioRepoMock.Setup(r => r.UpdateAsync(portfolio, _ct)).Returns(Task.CompletedTask);
            _portfolioRepoMock.Setup(r => r.SaveChangesAsync(_ct)).Returns(Task.CompletedTask);

            await _service.UpdateOwnerAsync(portfolio.Id, "NewOwner", _ct);

            portfolio.Owner.Should().Be("NewOwner");
            _portfolioRepoMock.Verify(r => r.UpdateAsync(portfolio, _ct), Times.Once);
            _portfolioRepoMock.Verify(r => r.SaveChangesAsync(_ct), Times.Once);
        }

        [Fact]
        public async Task UpdateOwnerAsync_Should_Do_Nothing_When_Portfolio_NotFound()
        {
            _portfolioRepoMock.Setup(r => r.GetByIdAsync(99, _ct)).ReturnsAsync((Portfolio?)null);

            await _service.UpdateOwnerAsync(99, "NewOwner", _ct);

            _portfolioRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Portfolio>(), _ct), Times.Never);
            _portfolioRepoMock.Verify(r => r.SaveChangesAsync(_ct), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_When_Portfolio_Exists()
        {
            var portfolio = TestEntityFactory.CreatePortfolio("Owner");

            _portfolioRepoMock.Setup(r => r.GetByIdAsync(portfolio.Id, _ct)).ReturnsAsync(portfolio);
            _portfolioRepoMock.Setup(r => r.DeleteAsync(portfolio, _ct)).Returns(Task.CompletedTask);
            _portfolioRepoMock.Setup(r => r.SaveChangesAsync(_ct)).Returns(Task.CompletedTask);

            await _service.DeleteAsync(portfolio.Id, _ct);

            _portfolioRepoMock.Verify(r => r.DeleteAsync(portfolio, _ct), Times.Once);
            _portfolioRepoMock.Verify(r => r.SaveChangesAsync(_ct), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_When_Portfolio_NotFound()
        {
            _portfolioRepoMock.Setup(r => r.GetByIdAsync(99, _ct)).ReturnsAsync((Portfolio?)null);

            await FluentActions.Awaiting(() => _service.DeleteAsync(99, _ct))
                               .Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_DTO_When_Found()
        {
            var portfolio = TestEntityFactory.CreatePortfolio("Owner");

            _portfolioRepoMock.Setup(r => r.GetByIdAsync(portfolio.Id, _ct)).ReturnsAsync(portfolio);

            var dto = await _service.GetByIdAsync(portfolio.Id, _ct);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(portfolio.Id);
            dto.Owner.Should().Be("Owner");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            _portfolioRepoMock.Setup(r => r.GetByIdAsync(99, _ct)).ReturnsAsync((Portfolio?)null);

            var dto = await _service.GetByIdAsync(99, _ct);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task ListAsync_Should_Return_All_Portfolios()
        {
            var p1 = TestEntityFactory.CreatePortfolio("Alice");
            var p2 = TestEntityFactory.CreatePortfolio("Bob");

            _portfolioRepoMock.Setup(r => r.ListAsync(_ct)).ReturnsAsync(new List<Portfolio> { p1, p2 });

            var list = await _service.ListAsync(_ct);

            list.Should().HaveCount(2);
            list.Select(x => x.Owner).Should().Contain(new[] { "Alice", "Bob" });
        }

        [Fact]
        public async Task GetAccountsByTagAsync_Should_Return_FilteredAccounts()
        {
            var tag = TestEntityFactory.CreateTag("RRSP");

            var account1 = TestEntityFactory.CreateAccount("Acc1", Currency.CAD);
            account1.AddTag(tag);

            var account2 = TestEntityFactory.CreateAccount("Acc2", Currency.USD);

            var portfolio = TestEntityFactory.CreatePortfolio("Owner");

            portfolio.AddAccount(account1);
            portfolio.AddAccount(account2);

            _portfolioRepoMock.Setup(r => r.GetByIdAsync(portfolio.Id, _ct)).ReturnsAsync(portfolio);

            var accounts = await _service.GetAccountsByTagAsync(portfolio.Id, tag, _ct);

            accounts.Should().HaveCount(1);
            accounts.Single().Name.Should().Be("Acc1");
        }

        [Fact]
        public async Task GetAccountsByTagAsync_Should_Return_Empty_When_Portfolio_NotFound()
        {
            var tag = TestEntityFactory.CreateTag("RRSP");
            _portfolioRepoMock.Setup(r => r.GetByIdAsync(99, _ct)).ReturnsAsync((Portfolio?)null);

            var accounts = await _service.GetAccountsByTagAsync(99, tag, _ct);

            accounts.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdWithIncludesAsync_Should_Return_DTO_With_Includes()
        {
            // Arrange
            var portfolio = TestEntityFactory.CreatePortfolio("Owner");

            var account = TestEntityFactory.CreateAccount("Acc1", Currency.CAD);
            portfolio.AddAccount(account);

            var includes = new IncludeOption[] { IncludeOption.Accounts };

            _portfolioRepoMock
                .Setup(r => r.GetByIdWithIncludesAsync(portfolio.Id, includes, _ct))
                .ReturnsAsync(portfolio);

            // Act
            var dto = await _service.GetByIdWithIncludesAsync(portfolio.Id, includes, _ct);

            // Assert
            dto.Should().NotBeNull();
            dto!.Id.Should().Be(portfolio.Id);
            dto.Accounts.Should().HaveCount(1);
            dto.Accounts.Single().Name.Should().Be("Acc1");
        }

        [Fact]
        public async Task GetByIdWithIncludesAsync_Should_Return_Null_When_Portfolio_NotFound()
        {
            var includes = new IncludeOption[] { IncludeOption.Accounts };
            _portfolioRepoMock.Setup(r => r.GetByIdWithIncludesAsync(99, includes, _ct))
                              .ReturnsAsync((Portfolio?)null);

            var dto = await _service.GetByIdWithIncludesAsync(99, includes, _ct);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task ListWithIncludesAsync_Should_Return_All_Portfolios_With_Includes()
        {
            // Arrange
            var p1 = TestEntityFactory.CreatePortfolio("Alice");
            var acc1 = TestEntityFactory.CreateAccount("AccA", Currency.CAD);
            p1.AddAccount(acc1);

            var p2 = TestEntityFactory.CreatePortfolio("Bob");
            var acc2 = TestEntityFactory.CreateAccount("AccB", Currency.USD);
            p2.AddAccount(acc2);

            var includes = new IncludeOption[] { IncludeOption.Accounts };

            _portfolioRepoMock
                .Setup(r => r.ListWithIncludesAsync(includes, _ct))
                .ReturnsAsync(new List<Portfolio> { p1, p2 });

            // Act
            var list = await _service.ListWithIncludesAsync(includes, _ct);

            // Assert
            list.Should().HaveCount(2);
            list.SelectMany(p => p.Accounts).Select(a => a.Name)
                .Should().Contain(new[] { "AccA", "AccB" });
        }

    }
}
