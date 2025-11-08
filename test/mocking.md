1️⃣ A clear, repeatable testing strategy for service classes that depend on other interfaces (mocking strategy, test coverage categories, conventions).
2️⃣ Then, the full test implementation for your PricingService following that pattern (mocking both IPriceService and IFxRateService, covering all cases).

🧭 1. Strategy for Testing Service Classes with Mocked Dependencies

You’ll encounter this pattern for most of your application services (PricingService, ValuationService, PortfolioService, etc.).

A. Use the “Arrange-Act-Assert” pattern with Moq

Each test should clearly separate:

Arrange: create the service, mock dependencies, prepare test data.

Act: call the method under test.

Assert: verify return values and mock interactions.

B. Mocking Guidelines

Use Moq for all mocks.

✅ Setup once per test case:

_mockPriceService = new Mock<IPriceService>();
_mockFxRateService = new Mock<IFxRateService>();
_sut = new PricingService(_mockPriceService.Object, _mockFxRateService.Object);


✅ Stub specific calls:

_mockPriceService
    .Setup(x => x.GetOrFetchInstrumentPriceAsync("VFV", date, It.IsAny<CancellationToken>()))
    .ReturnsAsync(new AssetPrice("VFV", new Money(150m, Currency.CAD), date));


✅ Verify expected calls:

_mockPriceService.Verify(x => x.GetOrFetchInstrumentPriceAsync("VFV", date, It.IsAny<CancellationToken>()), Times.Once);

C. Coverage Categories

For each service, you should test:

Category	What to Verify
Happy path	Expected values are computed correctly with typical data.
Currency conversion path	FX rate is applied correctly when reportingCurrency ≠ holding currency.
Edge cases	Missing price, zero quantity, null FX rate, cash vs non-cash.
Composition	Account and Portfolio methods aggregate correctly over holdings/accounts.
Error propagation	(Optional) Exceptions thrown from mocks propagate or are handled gracefully.

🔁 3. How to Apply This Strategy to Other Services

When testing any other service in your system:

Identify its collaborators (interfaces in constructor).

Create mocks for each.

Cover these 5 types of tests:

✅ Nominal (happy) path

⚖️ Boundary/edge cases (nulls, empty collections)

💱 Conversion/aggregation logic

⚡ Interaction verification (mock Verify)

💥 Error handling or null returns