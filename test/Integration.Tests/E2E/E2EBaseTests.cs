using System.Net.Http.Json;
using FluentAssertions;
using PM.DTO;
using PM.Integration.Tests;

namespace PM.Integration.E2E.Tests;

public abstract class E2EBaseTests : IClassFixture<E2EWebApplicationFactory>
{
    protected readonly E2EWebApplicationFactory _factory;
    protected readonly HttpClient _client;

    public E2EBaseTests(E2EWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public async Task<PortfolioDTO> CreatePortfolioAsync(string owner, CancellationToken ct = default)
    {
        var dto = new CreatePortfolioDTO(owner);

        var response = await _client.PostAsJsonAsync("/api/portfolios", dto, ct);
        response.EnsureSuccessStatusCode();

        var portfolio = await response.Content.ReadFromJsonAsync<PortfolioDTO>(cancellationToken: ct);
        portfolio.Should().NotBeNull($"Portfolio for {owner} should be created successfully.");

        return portfolio!;
    }

    public async Task<PortfolioDTO?> GetPortfolioAsync(int id, CancellationToken ct = default)
    {
        var response = await _client.GetAsync($"/api/portfolios/{id}", ct);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<PortfolioDTO>(cancellationToken: ct)
            : null;
    }

    public async Task<AccountDTO> AddAccountAsync(
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

    public async Task<AccountDTO?> GetAccountAsync(int portfolioId, int accountId, CancellationToken ct = default)
    {
        var response = await _client.GetAsync($"/api/portfolios/{portfolioId}/accounts/{accountId}", ct);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<AccountDTO>(cancellationToken: ct)
            : null;
    }

    public async Task DeletePortfolioAsync(int portfolioId, CancellationToken ct = default)
    {
        var response = await _client.DeleteAsync($"/api/portfolios/{portfolioId}", ct);
        // 204 No Content is expected for success
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent,
            $"Portfolio {portfolioId} should be deletable.");
    }

    public async Task RemoveAccountAsync(int portfolioId, int accountId, CancellationToken ct = default)
    {
        var response = await _client.DeleteAsync($"/api/portfolios/{portfolioId}/accounts/{accountId}", ct);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent,
            $"Account {accountId} in portfolio {portfolioId} should be removable.");
    }
}