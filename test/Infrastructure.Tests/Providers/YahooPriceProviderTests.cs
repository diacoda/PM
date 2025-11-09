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

public class YahooPriceProviderTests
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
    public async Task GetPriceAsync_ReturnsAssetPrice_WhenCloseAvailable()
    {
        // Arrange
        var symbol = new Symbol("VFV.TO", "CAD");
        var date = new DateOnly(2025, 1, 1);

        var json = @"{
            ""chart"": {
                ""result"": [{
                    ""timestamp"": [1735689600],
                    ""indicators"": { ""quote"": [{ ""close"": [100.5] }] }
                }]
            }
        }";

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        };

        var client = CreateMockHttpClient(response);
        var factory = CreateMockFactory(client);
        var provider = new YahooPriceProvider(factory);

        // Act
        var result = await provider.GetPriceAsync(symbol, date);

        // Assert
        result.Should().NotBeNull();
        result!.Symbol.Should().Be(symbol);
        result.Date.Should().Be(date);
        result.Price.Amount.Should().Be(100.5m);
        result.Price.Currency.Code.Should().Be("CAD");
        result.Source.Should().Be("Yahoo");
    }

    [Fact]
    public async Task GetPriceAsync_ReturnsNull_WhenCloseIsMissing()
    {
        var symbol = new Symbol("VFV.TO", "CAD");
        var date = new DateOnly(2025, 1, 1);

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
        var provider = new YahooPriceProvider(factory);

        var result = await provider.GetPriceAsync(symbol, date);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPriceAsync_Throws_WhenSymbolIsNull()
    {
        var factory = CreateMockFactory(new HttpClient());
        var provider = new YahooPriceProvider(factory);

        Func<Task> act = () => provider.GetPriceAsync(null!, new DateOnly(2025, 1, 1));
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetPriceAsync_ReturnsNull_WhenHttpFails()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var client = CreateMockHttpClient(response);
        var factory = CreateMockFactory(client);
        var provider = new YahooPriceProvider(factory);

        var symbol = new Symbol("VFV.TO", "CAD");
        var result = await provider.GetPriceAsync(symbol, new DateOnly(2025, 1, 1));

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPriceAsync_UsesCorrectTickerFormat()
    {
        // Arrange
        var capturedRequest = (HttpRequestMessage?)null;

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{""chart"":{""result"":[{""timestamp"":[1735689600],""indicators"":{""quote"":[{""close"":[100]}]}}]}}")
            });

        var client = new HttpClient(handlerMock.Object);
        var factory = CreateMockFactory(client);
        var provider = new YahooPriceProvider(factory);

        var symbol = new Symbol("VFV.TO", "CAD");
        var date = new DateOnly(2025, 1, 1);

        // Act
        await provider.GetPriceAsync(symbol, date);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString().Should().Contain("VFV.TO");
    }
}
