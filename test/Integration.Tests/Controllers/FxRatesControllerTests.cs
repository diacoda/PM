using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PM.API.Controllers;
using PM.Application.Interfaces;
using PM.Domain.Values;

namespace PM.Integration.Controllers.Tests;

public class FxRatesControllerTests
{
    private readonly Mock<IFxRateService> _fxServiceMock;
    private readonly FxRatesController _controller;

    public FxRatesControllerTests()
    {
        _fxServiceMock = new Mock<IFxRateService>();
        _controller = new FxRatesController(_fxServiceMock.Object);
    }

    [Fact]
    public async Task GetRate_ReturnsOk_WhenRateExists()
    {
        // Arrange
        var date = new DateOnly(2024, 05, 10);
        var fx = new FxRate(Currency.USD, Currency.CAD, date, 1.36m);
        _fxServiceMock.Setup(s => s.GetRateAsync("USD", "CAD", date, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(fx);

        // Act
        var result = await _controller.GetRate("USD", "CAD", "2024-05-10");

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(fx);
    }

    [Fact]
    public async Task GetRate_ReturnsNotFound_WhenRateDoesNotExist()
    {
        var date = new DateOnly(2024, 05, 10);
        _fxServiceMock.Setup(s => s.GetRateAsync("USD", "CAD", date, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((FxRate?)null);

        var result = await _controller.GetRate("USD", "CAD", "2024-05-10");

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetRate_ReturnsBadRequest_WhenDateIsInvalid()
    {
        var result = await _controller.GetRate("USD", "CAD", "bad-date");
        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.Value.Should().BeOfType<ProblemDetails>()
            .Which.Title.Should().Be("Invalid date format. Use YYYY-MM-DD.");
    }

    [Fact]
    public async Task GetRate_ReturnsBadRequest_WhenServiceThrowsArgumentException()
    {
        _fxServiceMock.Setup(s => s.GetRateAsync("USD", "CAD", It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new ArgumentException("Invalid currency"));

        var result = await _controller.GetRate("USD", "CAD", "2024-05-10");

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.Value.Should().BeOfType<ProblemDetails>()
            .Which.Title.Should().Be("Invalid currency");
    }

    [Fact]
    public async Task UpdateRate_ReturnsOk_WhenSuccessful()
    {
        var fx = new FxRate(Currency.USD, Currency.CAD, DateOnly.FromDateTime(DateTime.Today), 1.33m);
        _fxServiceMock.Setup(s => s.UpdateRateAsync("USD", "CAD", 1.33m, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(fx);

        var result = await _controller.UpdateRate("USD", "CAD", 1.33m);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(fx);
    }

    [Fact]
    public async Task UpdateRate_ReturnsBadRequest_WhenServiceThrowsArgumentException()
    {
        _fxServiceMock.Setup(s => s.UpdateRateAsync("USD", "CAD", 1.33m, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new ArgumentException("Invalid currency pair"));

        var result = await _controller.UpdateRate("USD", "CAD", 1.33m);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.Value.Should().BeOfType<ProblemDetails>()
            .Which.Title.Should().Be("Invalid currency pair");
    }

    [Fact]
    public async Task GetAllRatesForPair_ReturnsOk_WithRates()
    {
        var list = new List<FxRate>
        {
            new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 10), 1.35m),
            new FxRate(Currency.USD, Currency.CAD, new DateOnly(2024, 05, 11), 1.36m)
        };

        _fxServiceMock.Setup(s => s.GetAllRatesForPairAsync("USD", "CAD", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(list);

        var result = await _controller.GetAllRatesForPair("USD", "CAD");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetAllRatesForPair_ReturnsBadRequest_WhenServiceThrowsArgumentException()
    {
        _fxServiceMock.Setup(s => s.GetAllRatesForPairAsync("USD", "CAD", It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new ArgumentException("Invalid currency"));

        var result = await _controller.GetAllRatesForPair("USD", "CAD");

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.Value.Should().BeOfType<ProblemDetails>()
            .Which.Title.Should().Be("Invalid currency");
    }

    [Fact]
    public async Task DeleteRate_ReturnsNoContent_WhenDeleted()
    {
        var date = new DateOnly(2024, 05, 10);
        _fxServiceMock.Setup(s => s.DeleteRateAsync("USD", "CAD", date, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);

        var result = await _controller.DeleteRate("USD", "CAD", "2024-05-10");

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteRate_ReturnsNotFound_WhenRateNotFound()
    {
        var date = new DateOnly(2024, 05, 10);
        _fxServiceMock.Setup(s => s.DeleteRateAsync("USD", "CAD", date, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

        var result = await _controller.DeleteRate("USD", "CAD", "2024-05-10");

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteRate_ReturnsBadRequest_WhenDateInvalid()
    {
        var result = await _controller.DeleteRate("USD", "CAD", "invalid-date");

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.Value.Should().BeOfType<ProblemDetails>()
            .Which.Title.Should().Be("Invalid date format. Use YYYY-MM-DD.");
    }

    [Fact]
    public async Task DeleteRate_ReturnsBadRequest_WhenServiceThrowsArgumentException()
    {
        _fxServiceMock.Setup(s => s.DeleteRateAsync("USD", "CAD", It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new ArgumentException("Invalid currency pair"));

        var result = await _controller.DeleteRate("USD", "CAD", "2024-05-10");

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.Value.Should().BeOfType<ProblemDetails>()
            .Which.Title.Should().Be("Invalid currency pair");
    }

    [Fact]
    public async Task GetAllRatesByDate_ReturnsOk_WithRates()
    {
        var date = new DateOnly(2024, 05, 10);
        var list = new List<FxRate>
        {
            new FxRate(Currency.USD, Currency.CAD, date, 1.35m),
            new FxRate(Currency.EUR, Currency.CAD, date, 1.47m)
        };

        _fxServiceMock.Setup(s => s.GetAllRatesByDateAsync(date, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(list);

        var result = await _controller.GetAllRatesByDate("2024-05-10", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetAllRatesByDate_ReturnsBadRequest_WhenDateInvalid()
    {
        var result = await _controller.GetAllRatesByDate("bad-date", CancellationToken.None);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.Value.Should().BeOfType<ProblemDetails>()
            .Which.Title.Should().Be("Invalid date format. Use YYYY-MM-DD.");
    }
}
