using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using PM.Domain.Values;

namespace PM.Integration.Tests;

[Collection("IntegrationTests")]
public class FxRatesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public FxRatesControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRate_ShouldReturnOk_WhenRateExists()
    {
        // Arrange
        var from = "USD";

        var to = "CAD";
        var rate = 1.35m;
        var date = new DateOnly(2024, 05, 10);

        await _client.PutAsJsonAsync($"/api/fxrates/{from}/{to}?date={date:yyyy-MM-dd}", rate);

        // Act
        var response = await _client.GetAsync($"/api/fxrates/{from}/{to}/{date:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fxRate = await response.Content.ReadFromJsonAsync<FxRate>();
        fxRate.Should().NotBeNull();
        var fromCurrency = new Currency(from);
        fxRate!.FromCurrency.Should().Be(fromCurrency);
        var toCurrency = new Currency(to);
        fxRate.ToCurrency.Should().Be(toCurrency);
        fxRate.Rate.Should().Be(rate);
    }

    [Fact]
    public async Task GetRate_ShouldReturnNotFound_WhenRateDoesNotExist()
    {
        var response = await _client.GetAsync("/api/fxrates/USD/EUR/2024-05-10");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRate_ShouldReturnBadRequest_WhenInvalidDate()
    {
        var response = await _client.GetAsync("/api/fxrates/USD/CAD/invalid-date");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Contain("Invalid date format");
    }

    [Fact]
    public async Task UpdateRate_ShouldInsertOrUpdateFxRate()
    {
        // Arrange
        var from = "USD";
        var to = "CAD";
        var date = new DateOnly(2024, 01, 01);
        var rate = 1.32m;

        // Act
        var putResponse = await _client.PutAsJsonAsync($"/api/fxrates/{from}/{to}?date={date:yyyy-MM-dd}", rate);

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await putResponse.Content.ReadFromJsonAsync<FxRate>();
        created.Should().NotBeNull();
        var fromCurrency = new Currency(from);
        created!.FromCurrency.Should().Be(fromCurrency);
        var toCurrency = new Currency(to);
        created.ToCurrency.Should().Be(toCurrency);
        created.Rate.Should().Be(rate);
    }

    [Fact]
    public async Task GetAllRatesForPair_ShouldReturnList()
    {
        // Arrange
        var from = "USD";
        var to = "CAD";
        await _client.PutAsJsonAsync($"/api/fxrates/{from}/{to}?date=2024-01-01", 1.30m);
        await _client.PutAsJsonAsync($"/api/fxrates/{from}/{to}?date=2024-02-01", 1.31m);

        // Act
        var response = await _client.GetAsync($"/api/fxrates/{from}/{to}/history");

        // Assert
        var toCurrency = new Currency("CAD");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<FxRate>>();
        list.Should().NotBeNull();
        list!.Should().HaveCountGreaterThanOrEqualTo(2);
        list!.Select(x => x.ToCurrency)
            .Should()
            .AllSatisfy(c => c.Should().Be(toCurrency));

    }

    [Fact]
    public async Task GetAllRatesByDate_ShouldReturnAllRatesOnGivenDate()
    {
        // Arrange
        await _client.PutAsJsonAsync("/api/fxrates/USD/CAD?date=2024-03-01", 1.34m);
        await _client.PutAsJsonAsync("/api/fxrates/EUR/CAD?date=2024-03-01", 1.48m);

        // Act
        var response = await _client.GetAsync("/api/fxrates/date/2024-03-01");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<FxRate>>();
        list.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task DeleteRate_ShouldRemoveExistingFxRate()
    {
        // Arrange
        var from = "USD";
        var to = "CAD";
        var date = new DateOnly(2024, 01, 15);
        await _client.PutAsJsonAsync($"/api/fxrates/{from}/{to}?date={date:yyyy-MM-dd}", 1.25m);

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/fxrates/{from}/{to}/{date:yyyy-MM-dd}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/fxrates/{from}/{to}/{date:yyyy-MM-dd}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRate_ShouldReturnBadRequest_WhenInvalidDate()
    {
        var response = await _client.DeleteAsync("/api/fxrates/USD/CAD/not-a-date");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteRate_ShouldReturnNotFound_WhenRateMissing()
    {
        var response = await _client.DeleteAsync("/api/fxrates/USD/EUR/2024-09-01");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
