using PM.Integration.Tests;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PM.DTO;
using PM.Domain.Enums;
using Xunit;
using PM.Domain.Entities;

namespace PM.Integration.E2E.Tests;

public class PortfolioE2ETests : E2EBaseTests
{

    public PortfolioE2ETests(E2EWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task FullTransactionLifecycle_ShouldUpdatePortfolioCorrectly()
    {
        (PortfolioDTO portfolio, AccountDTO account) = await SetupPortfolioWithAccountAsync();

        var transactionDto = new CreateTransactionDTO();
        transactionDto.Date = DateOnly.FromDateTime(DateTime.UtcNow);
        transactionDto.Type = "Buy";
        transactionDto.Symbol = "VFV.TO";
        transactionDto.Quantity = 10m;
        transactionDto.Amount = 1500m;
        transactionDto.AmountCurrency = "CAD";
        transactionDto.Costs = 10m;
        transactionDto.CostsCurrency = "CAD";
        var postResponse = await _client.PostAsJsonAsync(
            $"/api/portfolios/{portfolio.Id}/accounts/{account.Id}/transactions",
            transactionDto);
        postResponse.EnsureSuccessStatusCode();
        var createdTransaction = await postResponse.Content.ReadFromJsonAsync<TransactionDTO>();
        createdTransaction.Should().NotBeNull("Transaction should be created successfully.");
        createdTransaction!.Type.Should().Be(TransactionType.Buy.ToString());

        var getResponse = await _client.GetAsync(
            $"/api/portfolios/{portfolio.Id}/accounts/{account.Id}/transactions/{createdTransaction.Id}");
        getResponse.EnsureSuccessStatusCode();
        var fetchedTransaction = await getResponse.Content.ReadFromJsonAsync<TransactionDTO>();
        fetchedTransaction.Should().NotBeNull("Transaction should be fetched successfully.");
        fetchedTransaction!.Id.Should().Be(createdTransaction.Id);
        fetchedTransaction.Type.Should().Be(TransactionType.Buy.ToString());

        // createdTransaction.CashFlowId
        // createdTransaction.HoldingIds

        await CleanupPortfolioAsync(portfolio, account);
    }
}