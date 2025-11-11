using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PM.DTO;
using PM.Domain.Enums;
using PM.Integration.Tests;
using PM.SharedKernel;

namespace PM.Integration.E2E.Tests;

public class PortfolioE2ETests : E2EBaseTests
{
    public PortfolioE2ETests(E2EWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task FullTransactionLifecycle_ShouldUpdatePortfolioCorrectly()
    {
        // Arrange
        var (portfolio, account) = await SetupPortfolioWithAccountAsync();

        try
        {
            // --- CREATE TRANSACTION ---
            var transactionDto = CreateBuyTransaction("VFV.TO", 10m, 150m);
            var createdTransaction = await CreateTransactionAsync(portfolio, account, transactionDto);

            createdTransaction.Should().NotBeNull();
            createdTransaction.Type.Should().Be(TransactionType.Buy.ToString());

            // --- GET TRANSACTION ---
            var fetchedTransaction = await GetTransactionAsync(portfolio, account, createdTransaction.Id);
            fetchedTransaction.Should().NotBeNull();
            fetchedTransaction.Should().BeEquivalentTo(createdTransaction, options => options
                .Excluding(x => x.CashFlowId) // allow backend-side generation differences
                .Excluding(x => x.HoldingIds) // allow backend-side generation differences
            );

            // --- VERIFY PORTFOLIO STATE AFTER BUY ---
            IncludeOption[] includes = new[] { IncludeOption.Accounts, IncludeOption.Holdings, IncludeOption.Transactions };
            var portfolioAfterBuy = await GetPortfolioAsync(portfolio.Id, includes);
            portfolioAfterBuy.Should().NotBeNull();
            portfolioAfterBuy!.Accounts.Should().ContainSingle(a => a.Id == account.Id);
            portfolioAfterBuy.Accounts.First(a => a.Id == account.Id).Holdings.Should().NotBeEmpty();

            // --- DELETE CASH FLOW ---
            var deleteResponse = await DeleteCashFlowAsync(createdTransaction.CashFlowId);
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // (Idon't have this, don't know I want to expose it) --- VERIFY CASH FLOW REMOVED ---
            //var checkDeleted = await _client.GetAsync($"/api/cashflows/{createdTransaction.CashFlowId}");
            //checkDeleted.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            await CleanupPortfolioAsync(portfolio, account);
        }
    }

    [Fact]
    public async Task DeleteCashFlow_ShouldReturnNotFound_ForNonexistentId()
    {
        var response = await _client.DeleteAsync("/api/cashflows/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
