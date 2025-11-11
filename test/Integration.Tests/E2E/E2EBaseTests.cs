using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PM.DTO;
using PM.Integration.Tests;
using PM.SharedKernel;

namespace PM.Integration.E2E.Tests;

[Collection("E2ETests")]
public abstract class E2EBaseTests : IClassFixture<E2EWebApplicationFactory>
{
    protected readonly E2EWebApplicationFactory _factory;
    protected readonly HttpClient _client;

    public E2EBaseTests(E2EWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    // ────────────────────────────────
    // Portfolio & Account Setup
    // ────────────────────────────────

    private async Task<PortfolioDTO> CreatePortfolioAsync(string owner, CancellationToken ct = default)
    {
        var dto = new CreatePortfolioDTO(owner);
        var response = await _client.PostAsJsonAsync("/api/portfolios", dto, ct);
        response.EnsureSuccessStatusCode();

        var portfolio = await response.Content.ReadFromJsonAsync<PortfolioDTO>(cancellationToken: ct);
        portfolio.Should().NotBeNull($"Portfolio for {owner} should be created successfully.");
        return portfolio!;
    }

    private async Task<AccountDTO> AddAccountAsync(
        int portfolioId,
        string name,
        string currency,
        string institution,
        CancellationToken ct = default)
    {
        var dto = new CreateAccountDTO(name, currency, institution);
        var response = await _client.PostAsJsonAsync($"/api/portfolios/{portfolioId}/accounts", dto, ct);
        response.EnsureSuccessStatusCode();

        var account = await response.Content.ReadFromJsonAsync<AccountDTO>(cancellationToken: ct);
        account.Should().NotBeNull($"Account {name} should be created successfully.");
        return account!;
    }

    protected async Task<(PortfolioDTO Portfolio, AccountDTO Account)> SetupPortfolioWithAccountAsync(
        string owner = "E2E Owner",
        string accountName = "E2E Account",
        string currency = "CAD",
        string institution = "TD",
        CancellationToken ct = default)
    {
        var portfolio = await CreatePortfolioAsync(owner, ct);
        var account = await AddAccountAsync(portfolio.Id, accountName, currency, institution, ct);
        return (portfolio, account);
    }

    protected async Task CleanupPortfolioAsync(PortfolioDTO portfolio, AccountDTO? account = null, CancellationToken ct = default)
    {
        if (account is not null)
            await RemoveAccountAsync(portfolio.Id, account.Id, ct);

        await DeletePortfolioAsync(portfolio.Id, ct);
    }

    private async Task RemoveAccountAsync(int portfolioId, int accountId, CancellationToken ct = default)
    {
        var response = await _client.DeleteAsync($"/api/portfolios/{portfolioId}/accounts/{accountId}", ct);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            $"Account {accountId} in portfolio {portfolioId} should be removable.");
    }

    private async Task DeletePortfolioAsync(int portfolioId, CancellationToken ct = default)
    {
        var response = await _client.DeleteAsync($"/api/portfolios/{portfolioId}", ct);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            $"Portfolio {portfolioId} should be deletable.");
    }

    // ────────────────────────────────
    // Transaction Helpers
    // ────────────────────────────────

    protected CreateTransactionDTO CreateBuyTransaction(string symbol = "VFV.TO", decimal qty = 10m, decimal price = 150m)
        => new()
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Type = "Buy",
            Symbol = symbol,
            Quantity = qty,
            Amount = qty * price,
            AmountCurrency = "CAD",
            Costs = 10m,
            CostsCurrency = "CAD"
        };

    protected async Task<TransactionDTO> CreateTransactionAsync(
        PortfolioDTO portfolio, AccountDTO account, CreateTransactionDTO dto, CancellationToken ct = default)
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/portfolios/{portfolio.Id}/accounts/{account.Id}/transactions", dto, ct);
        response.EnsureSuccessStatusCode();

        var tx = await response.Content.ReadFromJsonAsync<TransactionDTO>(cancellationToken: ct);
        tx.Should().NotBeNull("Transaction should be created successfully.");
        return tx!;
    }

    protected async Task<TransactionDTO> GetTransactionAsync(
        PortfolioDTO portfolio, AccountDTO account, int transactionId, CancellationToken ct = default)
    {
        var response = await _client.GetAsync(
            $"/api/portfolios/{portfolio.Id}/accounts/{account.Id}/transactions/{transactionId}", ct);
        response.EnsureSuccessStatusCode();

        var tx = await response.Content.ReadFromJsonAsync<TransactionDTO>(cancellationToken: ct);
        tx.Should().NotBeNull("Transaction should be fetched successfully.");
        return tx!;
    }

    protected async Task<HttpResponseMessage> DeleteCashFlowAsync(int cashFlowId, CancellationToken ct = default)
    {
        var response = await _client.DeleteAsync($"/api/cashflows/{cashFlowId}", ct);
        return response;
    }

    // ────────────────────────────────
    // Portfolio Retrieval
    // ────────────────────────────────

    protected async Task<PortfolioDTO?> GetPortfolioAsync(
    int id,
    IncludeOption[]? includes = null,
    CancellationToken ct = default)
    {
        var query = includes is { Length: > 0 }
            ? string.Join("&", includes.Select(i => $"include={i}"))
            : string.Empty;

        var url = string.IsNullOrEmpty(query)
            ? $"/api/portfolios/{id}"
            : $"/api/portfolios/{id}?{query}";

        var response = await _client.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<PortfolioDTO>(cancellationToken: ct);
    }

    protected async Task<IEnumerable<AccountDTO>?> GetAccountsAsync(
        int portfolioId,
        IncludeOption[]? includes = null,
        CancellationToken ct = default)
    {
        var query = includes is { Length: > 0 }
            ? string.Join("&", includes.Select(i => $"include={i}"))
            : string.Empty;

        var url = string.IsNullOrEmpty(query)
            ? $"/api/portfolios/{portfolioId}/accounts"
            : $"/api/portfolios/{portfolioId}/accounts?{query}";

        var response = await _client.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<IEnumerable<AccountDTO>>(cancellationToken: ct);
    }

    protected async Task<AccountDTO?> GetAccountAsync(
        int portfolioId,
        int accountId,
        IncludeOption[]? includes = null,
        CancellationToken ct = default)
    {
        var query = includes is { Length: > 0 }
            ? string.Join("&", includes.Select(i => $"include={i}"))
            : string.Empty;

        var url = string.IsNullOrEmpty(query)
            ? $"/api/portfolios/{portfolioId}/accounts/{accountId}"
            : $"/api/portfolios/{portfolioId}/accounts/{accountId}?{query}";

        var response = await _client.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<AccountDTO>(cancellationToken: ct);
    }

}
