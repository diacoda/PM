using System;
using System.Linq;
using Xunit;
using Moq;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.Domain.Interfaces;

namespace PM.Tests.Domain.Entities
{
    public class AccountTests
    {
        private Account CreateAccount(string name = "RRSP", string currencyCode = "CAD", FinancialInstitutions fi = FinancialInstitutions.TD)
        {
            return new Account(name, new Currency(currencyCode), fi);
        }

        private Holding CreateHolding(string code = "VFV.TO", decimal qty = 100)
        {
            var asset = new Asset
            {
                Code = code,
                Currency = new Currency("CAD"),
                AssetClass = AssetClass.USEquity
            };
            return new Holding(asset, qty);
        }

        [Fact]
        public void Constructor_ShouldInitializeAccount()
        {
            var account = CreateAccount();

            Assert.Equal("RRSP", account.Name);
            Assert.Equal("CAD", account.Currency.Code);
            Assert.Equal(FinancialInstitutions.TD, account.FinancialInstitution);
            Assert.Empty(account.Holdings);
            Assert.Empty(account.Tags);
            Assert.Empty(account.Transactions);
        }

        [Fact]
        public void UpdateName_ShouldChangeAccountName()
        {
            var account = CreateAccount();
            account.UpdateName("TFSA");

            Assert.Equal("TFSA", account.Name);
        }

        [Fact]
        public void AddTag_ShouldAddNewTag()
        {
            var account = CreateAccount();
            var tag = new Tag("Retirement");

            account.AddTag(tag);

            Assert.Contains(tag, account.Tags);
        }

        [Fact]
        public void AddTag_ShouldNotAddDuplicateTag()
        {
            var account = CreateAccount();
            var tag = new Tag("Retirement");

            account.AddTag(tag);
            account.AddTag(tag);

            Assert.Single(account.Tags);
        }

        [Fact]
        public void RemoveTag_ShouldRemoveExistingTag()
        {
            var account = CreateAccount();
            var tag = new Tag("Retirement");

            account.AddTag(tag);
            account.RemoveTag(tag);

            Assert.DoesNotContain(tag, account.Tags);
        }

        [Fact]
        public void UpsertHolding_ShouldAddNewHolding()
        {
            var account = CreateAccount();
            var holding = CreateHolding();

            var result = account.UpsertHolding(holding);

            Assert.Contains(holding, account.Holdings);
            Assert.Equal(holding, result);
        }

        [Fact]
        public void UpsertHolding_ShouldMergeQuantityForExistingHolding()
        {
            var account = CreateAccount();
            var holding1 = CreateHolding("VFV.TO", 100);
            var holding2 = CreateHolding("VFV.TO", 50);

            account.UpsertHolding(holding1);
            var result = account.UpsertHolding(holding2);

            Assert.Single(account.Holdings);
            Assert.Equal(150, result.Quantity);
        }

        [Fact]
        public void UpdateHoldingQuantity_ShouldUpdateExistingHolding()
        {
            var account = CreateAccount();
            var holding = CreateHolding("VFV.TO", 100);
            var upserted = account.UpsertHolding(holding);

            var updated = account.UpdateHoldingQuantity(new Symbol("VFV.TO", "CAD"), 200);

            Assert.Equal(200, updated.Quantity);
        }

        [Fact]
        public void UpdateHoldingQuantity_ShouldThrowIfHoldingNotFound()
        {
            var account = CreateAccount();

            Assert.Throws<ArgumentException>(() =>
                account.UpdateHoldingQuantity(new Symbol("XYZ", "TO"), 50));
        }

        [Fact]
        public void RemoveHolding_ShouldRemoveExistingHolding()
        {
            var account = CreateAccount();
            var holding = CreateHolding();
            account.UpsertHolding(holding);

            account.RemoveHolding(holding);

            Assert.DoesNotContain(holding, account.Holdings);
        }

        [Fact]
        public void AddTransaction_ShouldAddTransaction()
        {
            var account = CreateAccount();
            var currency = new Currency("CAD");
            var symbol = new Symbol("CAD");
            var transaction = new Transaction(1, TransactionType.Deposit, symbol, 100, new Money(100, new Currency("CAD")), DateOnly.FromDateTime(DateTime.Now));

            account.AddTransaction(transaction);

            Assert.Contains(transaction, account.Transactions);
        }

        [Fact]
        public void RemoveTransaction_ShouldRemoveTransaction()
        {
            var account = CreateAccount();
            var currency = new Currency("CAD");
            var symbol = new Symbol("CAD");
            var transaction = new Transaction(1, TransactionType.Deposit, symbol, 100, new Money(100, new Currency("CAD")), DateOnly.FromDateTime(DateTime.Now));

            account.AddTransaction(transaction);
            account.RemoveTransaction(transaction);

            Assert.DoesNotContain(transaction, account.Transactions);
        }

        [Fact]
        public void GetCashBalance_ShouldReturnZeroIfNoCashHolding()
        {
            var account = CreateAccount();
            var balance = account.GetCashBalance(new Currency("CAD"));

            Assert.Equal(0, balance);
        }

        [Fact]
        public void LinkToPortfolio_ShouldSetPortfolioAndId()
        {
            var account = CreateAccount();
            var portfolio = new Portfolio("Person1");

            account.LinkToPortfolio(portfolio);

            Assert.Equal(portfolio, account.Portfolio);
            Assert.Equal(portfolio.Id, account.PortfolioId);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            var account = CreateAccount();
            var result = account.ToString();

            Assert.Contains("Account", result);
            Assert.Contains(account.Name, result);
            Assert.Contains(account.Currency.Code, result);
        }
    }
}