using System;
using Xunit;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Domain.Enums;

namespace PM.Tests.Domain.Entities
{
    public class TransactionTests
    {
        private Symbol CreateSymbol(string code = "VFV.TO", string currency = "CAD")
            => new Symbol(code, currency);

        private Money CreateMoney(decimal amount = 100m, string currency = "CAD")
            => new Money(amount, new Currency(currency));

        [Fact]
        public void Constructor_ShouldSetAllProperties()
        {
            var accountId = 1;
            var type = TransactionType.Buy;
            var symbol = CreateSymbol();
            var quantity = 50m;
            var amount = CreateMoney();
            var date = new DateOnly(2025, 11, 5);

            var transaction = new Transaction(accountId, type, symbol, quantity, amount, date);

            Assert.Equal(accountId, transaction.AccountId);
            Assert.Equal(type, transaction.Type);
            Assert.Equal(symbol, transaction.Symbol);
            Assert.Equal(quantity, transaction.Quantity);
            Assert.Equal(amount, transaction.Amount);
            Assert.Equal(date, transaction.Date);
        }

        [Fact]
        public void ParameterlessConstructor_ShouldCreateTransaction()
        {
            var transaction = new Transaction();
            Assert.NotNull(transaction);
        }

        [Fact]
        public void ToString_ShouldReturnExpectedFormat()
        {
            var transaction = new Transaction(
                accountId: 1,
                type: TransactionType.Sell,
                instrument: CreateSymbol("VCE.TO", "CAD"),
                quantity: 10m,
                amount: CreateMoney(500m, "CAD"),
                date: new DateOnly(2025, 11, 5)
            );

            var str = transaction.ToString();

            Assert.Contains("Sell", str);
            Assert.Contains("10", str);
            Assert.Contains("VCE.TO", str);
            Assert.Contains("500", str);
            Assert.Contains("2025-11-05", str); // US-style short date format may vary by culture
        }

        [Fact]
        public void Costs_Property_ShouldBeNullable()
        {
            var transaction = new Transaction();
            Assert.Null(transaction.Costs);

            transaction.Costs = CreateMoney(10m, "CAD");
            Assert.NotNull(transaction.Costs);
            Assert.Equal(10m, transaction.Costs!.Amount);
        }

        [Fact]
        public void Account_Property_ShouldBeNullable()
        {
            var transaction = new Transaction();
            Assert.Null(transaction.Account);

            var account = new Account("RRSP", new Currency("CAD"), FinancialInstitutions.TD);
            transaction.Account = account;
            Assert.Equal(account, transaction.Account);
        }
    }
}
