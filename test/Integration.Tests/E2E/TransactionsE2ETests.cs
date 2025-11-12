using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PM.DTO;
using PM.Domain.Enums;
using PM.Integration.Tests;
using PM.SharedKernel;

namespace PM.Integration.E2E.Tests;

public class TransactionsE2ETests : E2EBaseTests
{
    public TransactionsE2ETests(E2EWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Day_By_Day()
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
