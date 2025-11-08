using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Domain.Enums;
using PM.Infrastructure.Services;
using Xunit;
using PM.Infrastructure.Tests.Utils;

namespace PM.Infrastructure.Services.Tests
{
    public class PricingServiceTests
    {
        private readonly Mock<IPriceService> _priceService;
        private readonly Mock<IFxRateService> _fxRateService;
        private readonly PricingService _sut;
        private readonly DateOnly _date = new(2025, 1, 1);

        public PricingServiceTests()
        {
            _priceService = new Mock<IPriceService>();
            _fxRateService = new Mock<IFxRateService>();
            _sut = new PricingService(_priceService.Object, _fxRateService.Object);
        }

        private static Holding CreateHolding(string code, string currencyCode, decimal quantity)
        {
            var asset = new Asset() { Code = code, AssetClass = AssetClass.Other, Currency = new Currency(currencyCode) };
            return new Holding(asset, quantity);
        }

        // 🧭 1️⃣ Cash holding in same currency (no FX)
        [Fact]
        public async Task CalculateHoldingValueAsync_ShouldReturnSameAmount_ForCashInReportingCurrency()
        {
            var holding = CreateHolding("CAD", "CAD", 1000m);

            var result = await _sut.CalculateHoldingValueAsync(holding, _date, Currency.CAD);

            result.Should().Be(new Money(1000m, Currency.CAD));
            _fxRateService.VerifyNoOtherCalls();
            _priceService.VerifyNoOtherCalls();
        }

        // 🧭 2️⃣ Cash holding with FX (USD → CAD)
        [Fact]
        public async Task CalculateHoldingValueAsync_ShouldConvertCurrency_ForCashHoldingWithDifferentCurrency()
        {
            var holding = CreateHolding("USD", "USD", 1000m);

            _fxRateService.Setup(x =>
                x.GetRateAsync("USD", "CAD", _date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FxRate(Currency.USD, Currency.CAD, _date, 1.35m));

            var result = await _sut.CalculateHoldingValueAsync(holding, _date, Currency.CAD);

            result.Should().Be(new Money(1350m, Currency.CAD));
        }

        // 🧭 3️⃣ Equity holding with price in same currency
        [Fact]
        public async Task CalculateHoldingValueAsync_ShouldReturnValue_WhenPriceInSameCurrency()
        {
            var holding = CreateHolding("VFV.TO", "CAD", 10m);

            _priceService.Setup(x =>
                x.GetOrFetchInstrumentPriceAsync("VFV.TO", _date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AssetPrice(new Symbol("VFV.TO"), _date, new Money(150m, Currency.CAD), "Test"));

            var result = await _sut.CalculateHoldingValueAsync(holding, _date, Currency.CAD);

            result.Should().Be(new Money(1500m, Currency.CAD));
        }

        // 🧭 4️⃣ Equity holding with FX conversion
        [Fact]
        public async Task CalculateHoldingValueAsync_ShouldApplyFx_WhenPriceCurrencyDiffers()
        {
            var holding = CreateHolding("VFV.TO", "CAD", 5m);

            _priceService.Setup(x =>
                x.GetOrFetchInstrumentPriceAsync("VFV.TO", _date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AssetPrice(new Symbol("VFV.TO"), _date, new Money(200m, Currency.USD)));

            _fxRateService.Setup(x =>
                x.GetRateAsync("USD", "CAD", _date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FxRate(Currency.USD, Currency.CAD, _date, 1.35m));

            var result = await _sut.CalculateHoldingValueAsync(holding, _date, Currency.CAD);

            result.Should().Be(new Money(1350m, Currency.CAD));
        }

        // 🧭 5️⃣ Missing price
        [Fact]
        public async Task CalculateHoldingValueAsync_ShouldReturnZero_WhenPriceMissing()
        {
            var holding = CreateHolding("VFV.TO", "CAD", 10m);

            _priceService.Setup(x =>
                x.GetOrFetchInstrumentPriceAsync("VFV.TO", _date, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AssetPrice?)null);

            var result = await _sut.CalculateHoldingValueAsync(holding, _date, Currency.CAD);

            result.Should().Be(new Money(0m, Currency.CAD));
        }

        // 🧭 6️⃣ Account value aggregation
        [Fact]
        public async Task CalculateAccountValueAsync_ShouldSumHoldingsValues()
        {
            var account = new Account("Test Account", Currency.CAD, FinancialInstitutions.TD);
            account.UpsertHolding(CreateHolding("CAD", "CAD", 1000m));
            account.UpsertHolding(CreateHolding("VFV.TO", "CAD", 10m));

            _priceService.Setup(x =>
                x.GetOrFetchInstrumentPriceAsync("VFV.TO", _date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AssetPrice(new Symbol("VFV.TO"), _date, new Money(150m, Currency.CAD)));

            var result = await _sut.CalculateAccountValueAsync(account, _date, Currency.CAD);

            result.Should().Be(new Money(2500m, Currency.CAD));
        }

        // 🧭 7️⃣ Portfolio aggregation across multiple accounts
        [Fact]
        public async Task CalculatePortfolioValueAsync_ShouldSumAccountsValues()
        {
            var portfolio = new Portfolio("P1");

            var acc1 = TestEntityFactory.CreateAccount("A1", Currency.CAD, FinancialInstitutions.TD);
            acc1.UpsertHolding(CreateHolding("CAD", "CAD", 1000m));

            var acc2 = TestEntityFactory.CreateAccount("A1", Currency.CAD, FinancialInstitutions.TD);
            acc2.UpsertHolding(CreateHolding("VFV.TO", "CAD", 10m));

            portfolio.AddAccount(acc1);
            portfolio.AddAccount(acc2);

            _priceService.Setup(x =>
                x.GetOrFetchInstrumentPriceAsync("VFV.TO", _date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AssetPrice(new Symbol("VFV.TO"), _date, new Money(150m, Currency.CAD)));

            var result = await _sut.CalculatePortfolioValueAsync(portfolio, _date, Currency.CAD);

            result.Should().Be(new Money(2500m, Currency.CAD));
        }

    }
}
