using PM.Integration.Tests;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PM.DTO;
using PM.Domain.Enums;
using Xunit;

namespace PM.Integration.E2E.Tests;

public class PortfolioE2ETests : E2EBaseTests
{

    public PortfolioE2ETests(E2EWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task FullTransactionLifecycle_ShouldUpdatePortfolioCorrectly()
    {
        // 1. Create account
        var portfolio = await base.CreatePortfolioAsync("E2E User");
        var account = await base.AddAccountAsync(portfolio.Id, "E2E Account", "CAD", "TD");

        await base.RemoveAccountAsync(portfolio.Id, account.Id);
        var removedAccount = await base.GetAccountAsync(portfolio.Id, account.Id);
        removedAccount.Should().BeNull("Account should be removed successfully.");

        await base.DeletePortfolioAsync(portfolio.Id);
        var deletedPortfolio = await base.GetPortfolioAsync(portfolio.Id);
        deletedPortfolio.Should().BeNull("Portfolio should be deleted successfully.");
    }
}