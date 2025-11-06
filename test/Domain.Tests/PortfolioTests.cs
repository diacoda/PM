using System;
using System.Linq;
using Xunit;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Domain.Enums;

namespace PM.Tests.Domain.Entities
{
    public class PortfolioTests
    {
        private Account CreateAccount(int id = 0, string name = "RRSP", string currencyCode = "CAD")
        {
            var account = new Account(name, new Currency(currencyCode), FinancialInstitutions.TD);
            if (id > 0)
            {
                // Simulate EF Core-generated ID
                typeof(Account).GetProperty("Id")!.SetValue(account, id);
            }
            return account;
        }

        [Fact]
        public void Constructor_ShouldTrimOwnerName()
        {
            var portfolio = new Portfolio("  Alice  ");
            Assert.Equal("Alice", portfolio.Owner);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_ShouldThrow_ForInvalidOwner(string? invalidOwner)
        {
            Assert.Throws<ArgumentException>(() => new Portfolio(invalidOwner!));
        }

        [Fact]
        public void AddAccount_ShouldAddAccount()
        {
            var portfolio = new Portfolio("Alice");
            var account = CreateAccount(1);

            portfolio.AddAccount(account);

            Assert.Contains(account, portfolio.Accounts);
            Assert.Equal(portfolio, account.Portfolio);
        }

        [Fact]
        public void AddAccount_ShouldThrow_WhenAccountIsNull()
        {
            var portfolio = new Portfolio("Alice");
            Assert.Throws<ArgumentNullException>(() => portfolio.AddAccount(null!));
        }

        [Fact]
        public void AddAccount_ShouldThrow_WhenDuplicateAccountId()
        {
            var portfolio = new Portfolio("Alice");
            var account1 = CreateAccount(1);
            var account2 = CreateAccount(1); // same Id

            portfolio.AddAccount(account1);

            var ex = Assert.Throws<InvalidOperationException>(() => portfolio.AddAccount(account2));
            Assert.Contains("already in this portfolio", ex.Message);
        }

        [Fact]
        public void RemoveAccount_ShouldRemoveAccount()
        {
            var portfolio = new Portfolio("Alice");
            var account = CreateAccount(1);
            portfolio.AddAccount(account);

            portfolio.RemoveAccount(account);

            Assert.DoesNotContain(account, portfolio.Accounts);
        }

        [Fact]
        public void RemoveAccount_ShouldThrow_WhenAccountIsNull()
        {
            var portfolio = new Portfolio("Alice");
            Assert.Throws<ArgumentNullException>(() => portfolio.RemoveAccount(null!));
        }

        [Fact]
        public void RemoveAccount_ShouldThrow_WhenAccountNotFound()
        {
            var portfolio = new Portfolio("Alice");
            var account = CreateAccount(1);
            var ex = Assert.Throws<InvalidOperationException>(() => portfolio.RemoveAccount(account));
            Assert.Contains("Account not found", ex.Message);
        }

        [Fact]
        public void ContainsAccount_ShouldReturnTrue_WhenAccountExists()
        {
            var portfolio = new Portfolio("Alice");
            var account = CreateAccount(42);
            portfolio.AddAccount(account);

            Assert.True(portfolio.ContainsAccount(42));
        }

        [Fact]
        public void ContainsAccount_ShouldReturnFalse_WhenAccountDoesNotExist()
        {
            var portfolio = new Portfolio("Alice");
            Assert.False(portfolio.ContainsAccount(99));
        }

        [Fact]
        public void Accounts_ShouldBeReadOnly()
        {
            var portfolio = new Portfolio("Alice");
            var account = CreateAccount(1);
            portfolio.AddAccount(account);

            var accounts = portfolio.Accounts;
            Assert.IsAssignableFrom<IReadOnlyCollection<Account>>(accounts);
            Assert.Throws<NotSupportedException>(() => ((System.Collections.IList)accounts).Add(CreateAccount()));
        }

        [Fact]
        public void ToString_ShouldIncludeOwnerAndAccountCount()
        {
            var portfolio = new Portfolio("Alice");
            Assert.Equal("Alice's Portfolio (Accounts: 0)", portfolio.ToString());

            portfolio.AddAccount(CreateAccount(1));
            portfolio.AddAccount(CreateAccount(2));

            Assert.Equal("Alice's Portfolio (Accounts: 2)", portfolio.ToString());
        }
    }
}
