using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public class ReportingServiceTests
    {
        private readonly Mock<IPricingService> _mockPricing;
        private readonly ReportingService _service;

        public ReportingServiceTests()
        {
            _mockPricing = new Mock<IPricingService>();
            _service = new ReportingService(_mockPricing.Object);
        }

        private static Account CreateTestAccount(string name = "Test Account")
        {
            var cad = new Currency("CAD");
            var asset1 = new Symbol("VFV.TO", "CAD");
            var asset2 = new Symbol("CAD", "CAD");

            var account = TestEntityFactory.CreateAccount(name, cad);
            account.UpsertHolding(new Holding(asset1, 1000m));
            account.UpsertHolding(new Holding(asset2, 50m));


            // Add Transactions
            account.AddTransaction(new Transaction
            {
                Date = new DateOnly(2025, 1, 1),
                Type = TransactionType.Buy,
                Symbol = asset1,
                Quantity = 50m,
                Amount = new Money(7500m, cad),
                Costs = new Money(10m, cad)
            });

            account.AddTransaction(new Transaction
            {
                Date = new DateOnly(2025, 1, 2),
                Type = TransactionType.Sell,
                Symbol = asset1,
                Quantity = 30m,
                Amount = new Money(5000m, cad),
                Costs = new Money(10m, cad)
            });

            account.AddTransaction(new Transaction
            {
                Date = new DateOnly(2025, 1, 3),
                Type = TransactionType.Dividend,
                Symbol = asset1,
                Quantity = 0,
                Amount = new Money(100m, cad),
                Costs = new Money(5m, cad)
            });

            account.AddTransaction(new Transaction
            {
                Date = new DateOnly(2025, 1, 3),
                Type = TransactionType.Interest,
                Symbol = asset2,
                Quantity = 0,
                Amount = new Money(50m, cad),
                Costs = new Money(0m, cad)
            });

            return account;
        }

        [Fact]
        public async Task AggregateByAssetClassAsync_Account_ReturnsCorrectSum()
        {
            var account = CreateTestAccount();

            _mockPricing
                .Setup(p => p.CalculateHoldingValueAsync(It.IsAny<Holding>(), It.IsAny<DateOnly>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Holding h, DateOnly d, Currency c, CancellationToken _) => new Money(h.Quantity * 100, c));

            var result = await _service.AggregateByAssetClassAsync(account, new DateOnly(2025, 1, 1), account.Currency);

            // We only have two equity assets
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey(AssetClass.USEquity));
            Assert.Equal(1000 * 100, result[AssetClass.USEquity].Amount);
        }

        [Fact]
        public async Task GetAssetClassPercentagesAsync_Account_ReturnsCorrectPercentages()
        {
            var account = CreateTestAccount();

            _mockPricing
                .Setup(p => p.CalculateHoldingValueAsync(It.IsAny<Holding>(), It.IsAny<DateOnly>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Holding h, DateOnly d, Currency c, CancellationToken _) => new Money(h.Quantity, c));

            var result = await _service.GetAssetClassPercentagesAsync(account, new DateOnly(2025, 1, 1), account.Currency);

            var total = account.Holdings.Sum(h => h.Quantity);
            foreach (var h in account.Holdings)
            {
                Assert.Equal(h.Quantity / total, result[h.Asset.AssetClass]);
            }
        }

        [Fact]
        public void GetTradingCostsByCurrency_Account_ReturnsCorrectAmounts()
        {
            var account = CreateTestAccount();
            var costs = _service.GetTradingCostsByCurrency(account, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5));

            Assert.Single(costs);
            Assert.Equal(25m, costs[account.Currency]); // 10 + 10 + 5
        }

        [Fact]
        public void GetTransactionCostSummaries_Account_CalculatesCorrectly()
        {
            var account = CreateTestAccount();
            var summaries = _service.GetTransactionCostSummaries(account, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5)).ToList();

            Assert.Single(summaries);
            var summary = summaries[0];

            Assert.Equal(1, summary.BuyCount);
            Assert.Equal(1, summary.SellCount);
            Assert.Equal(1, summary.DividendCount);
            Assert.Equal(0, summary.InterestCount);
            Assert.Equal(10m, summary.BuyCosts);
            Assert.Equal(10m, summary.SellCosts);
            Assert.Equal(5m, summary.DividendWithholding);
        }

        [Fact]
        public void PrintTransactionCostReport_Account_WritesOutput()
        {
            var account = CreateTestAccount();
            using var sw = new StringWriter();
            Console.SetOut(sw);

            _service.PrintTransactionCostReport(account, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5));

            var output = sw.ToString();
            Assert.Contains("Total Costs", output);
            Assert.Contains("Buys", output);
            Assert.Contains("Sells", output);
            Assert.Contains("Dividends", output);
        }

        [Fact]
        public void GetTransactionCostsBySecurity_Account_ReturnsCorrectGrouping()
        {
            var account = CreateTestAccount();
            var costs = _service.GetTransactionCostsBySecurity(account, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 5)).ToList();

            Assert.Equal(3, costs.Count); // Buy, Sell, Dividend, Interest
            var buy = costs.FirstOrDefault(c => c.Type == TransactionType.Buy);
            Assert.NotNull(buy);
            Assert.Equal(10m, buy.TotalCosts);
        }

        [Fact]
        public async Task PortfolioAggregation_WorksCorrectly()
        {
            var account1 = CreateTestAccount();
            var account2 = CreateTestAccount("New Account");

            var portfolio = new Portfolio { Owner = "Test Owner" };
            portfolio.AddAccount(account1);
            portfolio.AddAccount(account2);

            _mockPricing
                .Setup(p => p.CalculateHoldingValueAsync(It.IsAny<Holding>(), It.IsAny<DateOnly>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Holding h, DateOnly d, Currency c, CancellationToken _) => new Money(h.Quantity, c));

            var agg = await _service.AggregateByAssetClassAsync(portfolio, new DateOnly(2025, 1, 1), account1.Currency);

            Assert.Equal(2, agg.Count); // Two asset classes
            Assert.Equal(2000, agg[AssetClass.USEquity].Amount);
            Assert.Equal(100, agg[AssetClass.Cash].Amount);
        }
    }
}