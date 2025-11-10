using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PM.DTO;
using PM.Domain.Enums;
using Xunit;

namespace PM.Integration.Tests;

public class TransactionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TransactionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<AccountDTO> CreateTestAccountAsync(string name = "TestRRSP", string currency = "CAD")
    {
        var portfolioDTO = new CreatePortfolioDTO("IntegrationUser");
        var portfolioResponse = await _client.PostAsJsonAsync("/api/portfolios", portfolioDTO);
        portfolioResponse.EnsureSuccessStatusCode();
        var portfolio = await portfolioResponse.Content.ReadFromJsonAsync<PortfolioDTO>();

        var accountDto = new
        {
            Name = name,
            Currency = currency,
            FinancialInstitution = "TD"
        };

        var response = await _client.PostAsJsonAsync($"/api/portfolios/{portfolio!.Id}/accounts", accountDto);
        response.EnsureSuccessStatusCode();
        var account = await response.Content.ReadFromJsonAsync<AccountDTO>();
        return account ?? throw new InvalidOperationException("Failed to deserialize account response");
    }

    [Fact]
    public async Task Deposit_ShouldReturnTransactionDto()
    {
        var account = await CreateTestAccountAsync();

        CashFlowDTO cashFlowDTO = new CashFlowDTO(5000, "CAD", DateOnly.FromDateTime(DateTime.UtcNow), "Initial deposit");

        var response = await _client.PostAsJsonAsync($"/api/portfolios/{account.PortfolioId}/accounts/{account.Id}/transactions/deposit", cashFlowDTO);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var tx = await response.Content.ReadFromJsonAsync<TransactionDTO>();
        tx.Should().NotBeNull();
        tx!.Type.Should().Be("Deposit");
        tx.Amount.Should().Be(5000);
        tx.AccountId.Should().Be(account.Id);
    }

    [Fact]
    public async Task Withdraw_ShouldReturnTransactionDto()
    {
        var account = await CreateTestAccountAsync();
        CashFlowDTO cashFlowDTO = new CashFlowDTO(5000, "CAD", DateOnly.FromDateTime(DateTime.UtcNow), "Initial deposit");
        // Deposit first
        await _client.PostAsJsonAsync($"/api/portfolios/{account.PortfolioId}/accounts/{account.Id}/transactions/deposit",
            cashFlowDTO, CancellationToken.None);

        var withdrawal = new CashFlowDTO(500, "CAD", DateOnly.FromDateTime(DateTime.UtcNow), "Withdrawal test");
        var response = await _client.PostAsJsonAsync($"/api/portfolios/{account.PortfolioId}/accounts/{account.Id}/transactions/withdraw", withdrawal);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tx = await response.Content.ReadFromJsonAsync<TransactionDTO>();
        tx.Should().NotBeNull();
        tx!.Type.Should().Be("Withdrawal");
        tx.Amount.Should().Be(500);
    }

    [Fact]
    public async Task CreateTransaction_ShouldReturnTransactionDto()
    {
        var account = await CreateTestAccountAsync();

        var txDto = new CreateTransactionDTO
        {
            Type = TransactionType.Buy.ToString(),
            Symbol = "VFV.TO",
            Quantity = 10,
            Amount = 1000,
            AmountCurrency = "CAD"
        };

        var response = await _client.PostAsJsonAsync($"/api/portfolios/{account.PortfolioId}/accounts/{account.Id}/transactions", txDto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var tx = await response.Content.ReadFromJsonAsync<TransactionDTO>();
        tx.Should().NotBeNull();
        tx!.Type.Should().Be("Buy");
        tx.Quantity.Should().Be(10);
        tx.Amount.Should().Be(1000);
        tx.AccountId.Should().Be(account.Id);
    }

    [Fact]
    public async Task GetTransaction_ShouldReturnTransactionDto()
    {
        var account = await CreateTestAccountAsync();

        // Create transaction
        var txDto = new CreateTransactionDTO
        {
            Type = TransactionType.Buy.ToString(),
            Symbol = "VFV.TO",
            Quantity = 5,
            Amount = 500,
            AmountCurrency = "CAD"
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/portfolios/{account.PortfolioId}/accounts/{account.Id}/transactions", txDto);
        var txCreated = await createResponse.Content.ReadFromJsonAsync<TransactionDTO>();

        var getResponse = await _client.GetAsync($"/api/portfolios/{account.PortfolioId}/accounts/{account.Id}/transactions/{txCreated!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var txFetched = await getResponse.Content.ReadFromJsonAsync<TransactionDTO>();
        txFetched.Should().NotBeNull();
        txFetched!.Id.Should().Be(txCreated.Id);
        txFetched.Type.Should().Be("Buy");
    }

    [Fact]
    public async Task GetTransactionsList_ShouldReturnAllTransactions()
    {
        var account = await CreateTestAccountAsync();

        // Create multiple transactions
        CashFlowDTO cashFlowDTO1 = new CashFlowDTO(1000, "CAD", DateOnly.FromDateTime(DateTime.UtcNow), "");
        CashFlowDTO cashFlowDTO2 = new CashFlowDTO(2000, "CAD", DateOnly.FromDateTime(DateTime.UtcNow), "");
        await _client.PostAsJsonAsync($"/api/portfolios/{account.PortfolioId}/accounts/{account.Id}/transactions/deposit",
            cashFlowDTO1);
        await _client.PostAsJsonAsync($"/api/portfolios/{account.PortfolioId}/accounts/{account.Id}/transactions/deposit",
            cashFlowDTO2);

        var response = await _client.GetAsync($"/api/portfolios/{account.PortfolioId}/accounts/{account.Id}/transactions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var txList = await response.Content.ReadFromJsonAsync<List<TransactionDTO>>();
        txList.Should().NotBeNull();
        txList!.Count.Should().BeGreaterOrEqualTo(2);
        txList.Select(t => t.Amount).Should().Contain(new[] { 1000m, 2000m });
    }
}
