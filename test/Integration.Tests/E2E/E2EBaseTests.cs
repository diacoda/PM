using System.Net.Http.Json;
using FluentAssertions;
using PM.DTO;
using PM.Integration.Tests;

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

    private async Task<PortfolioDTO> CreatePortfolioAsync(string owner, CancellationToken ct = default)
    {
        var dto = new CreatePortfolioDTO(owner);

        var response = await _client.PostAsJsonAsync("/api/portfolios", dto, ct);
        response.EnsureSuccessStatusCode();

        var portfolio = await response.Content.ReadFromJsonAsync<PortfolioDTO>(cancellationToken: ct);
        portfolio.Should().NotBeNull($"Portfolio for {owner} should be created successfully.");

        return portfolio!;
    }

    protected async Task<PortfolioDTO?> GetPortfolioAsync(int id, CancellationToken ct = default)
    {
        var response = await _client.GetAsync($"/api/portfolios/{id}", ct);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<PortfolioDTO>(cancellationToken: ct)
            : null;
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

    protected async Task<AccountDTO?> GetAccountAsync(int portfolioId, int accountId, CancellationToken ct = default)
    {
        var response = await _client.GetAsync($"/api/portfolios/{portfolioId}/accounts/{accountId}", ct);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<AccountDTO>(cancellationToken: ct)
            : null;
    }

    private async Task DeletePortfolioAsync(int portfolioId, CancellationToken ct = default)
    {
        var response = await _client.DeleteAsync($"/api/portfolios/{portfolioId}", ct);
        // 204 No Content is expected for success
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent,
            $"Portfolio {portfolioId} should be deletable.");
    }

    private async Task RemoveAccountAsync(int portfolioId, int accountId, CancellationToken ct = default)
    {
        var response = await _client.DeleteAsync($"/api/portfolios/{portfolioId}/accounts/{accountId}", ct);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent,
            $"Account {accountId} in portfolio {portfolioId} should be removable.");
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
        {
            await RemoveAccountAsync(portfolio.Id, account.Id, ct);
        }

        await DeletePortfolioAsync(portfolio.Id, ct);
    }
}