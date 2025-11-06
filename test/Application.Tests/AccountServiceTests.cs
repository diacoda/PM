using Xunit;
using Moq;
using FluentAssertions;
using PM.Application.Services;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Domain.Enums;
using System.Threading.Tasks;

namespace Application.Tests
{
    public class AccountServiceTests
    {
        [Fact]
        public async Task CreateAsync_Should_LinkAccountToPortfolio()
        {
            var portfolio = new Portfolio("Owner");
            var mockPortfolioRepo = new Mock<IPortfolioRepository>();
            var mockAccountRepo = new Mock<IAccountRepository>();

            mockPortfolioRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), default))
                             .ReturnsAsync(portfolio);

            var service = new AccountService(mockAccountRepo.Object, mockPortfolioRepo.Object);

            var account = await service.CreateAsync(1, "Cash", Currency.CAD, FinancialInstitutions.TD);

            account.PortfolioId.Should().Be(portfolio.Id);
            account.Name.Should().Be("Cash");
        }
    }
}
