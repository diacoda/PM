using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using PM.Domain.Values;
using PM.Infrastructure.Providers;
using Xunit;

namespace PM.Infrastructure.Providers.Tests;

public class YahooFxProviderTests
{
    private HttpClient CreateMockHttpClient(HttpResponseMessage response)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response)
            .Verifiable();

        return new HttpClient(handlerMock.Object);
    }

    private IHttpClientFactory CreateMockFactory(HttpClient client)
    {
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        return factoryMock.Object;
    }

    [Fact]
    public async Task GetFxRateAsync_ReturnsFxRate_WhenCloseAvailable()
    {
        // Arrange
        var date = new DateOnly(2025, 1, 1);

        var json = @"{
            ""chart"": {
                ""result"": [{
                    ""timestamp"": [1735689600],
                    ""indicators"": { ""quote"": [{ ""close"": [1.35] }] }
                }]
            }
        }";

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        };

        var client = CreateMockHttpClient(response);
        var factory = CreateMockFactory(client);
        var provider = new YahooFxProvider(factory);

        // Act
        var result = await provider.GetFxRateAsync(Currency.USD, Currency.CAD, date);

        // Assert
        result.Should().NotBeNull();
        result!.FromCurrency.Should().Be(Currency.USD);
        result.ToCurrency.Should().Be(Currency.CAD);
        result.Date.Should().Be(date);
        result.Rate.Should().Be(1.35m);
    }

    [Fact]
    public async Task GetFxRateAsync_ReturnsNull_WhenNoDataForDate()
    {
        // Arrange: empty close array
        var json = @"{
            ""chart"": {
                ""result"": [{
                    ""timestamp"": [1735689600],
                    ""indicators"": { ""quote"": [{ ""close"": [null] }] }
                }]
            }
        }";

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        };

        var client = CreateMockHttpClient(response);
        var factory = CreateMockFactory(client);
        var provider = new YahooFxProvider(factory);

        // Act
        var result = await provider.GetFxRateAsync(Currency.USD, Currency.CAD, new DateOnly(2025, 1, 1));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFxRateAsync_Throws_WhenFromCurrencyIsNull()
    {
        var factory = CreateMockFactory(new HttpClient());
        var provider = new YahooFxProvider(factory);

        Func<Task> act = () => provider.GetFxRateAsync(null!, Currency.CAD, new DateOnly(2025, 1, 1));
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetFxRateAsync_Throws_WhenToCurrencyIsNull()
    {
        var factory = CreateMockFactory(new HttpClient());
        var provider = new YahooFxProvider(factory);

        Func<Task> act = () => provider.GetFxRateAsync(Currency.USD, null!, new DateOnly(2025, 1, 1));
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetFxRateAsync_ReturnsNull_WhenHttpFails()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var client = CreateMockHttpClient(response);
        var factory = CreateMockFactory(client);
        var provider = new YahooFxProvider(factory);

        var result = await provider.GetFxRateAsync(Currency.USD, Currency.CAD, new DateOnly(2025, 1, 1));
        result.Should().BeNull();
    }
}
