using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

using PM.Application.Interfaces; // IAccountService, IPortfolioService, IReportingService
using PM.Domain.Entities; // Account, Portfolio
using PM.Domain.Enums; // AssetClass, TransactionType, IncludeOption (assumed)
using PM.Domain.Mappers;
using PM.Domain.Values; // Money, Currency
using PM.DTO;
using PM.SharedKernel;

namespace PM.Api.Controllers;

/// <summary>
/// HTTP API endpoints that surface reporting features over Accounts and Portfolios.
/// Uses application services for reads (DTOs) and adapts them to domain models for ReportingService.
/// </summary>
[ApiController]
[Route("api/reporting")]
public sealed class ReportingController : ControllerBase
{
    private readonly IReportingService _reporting;
    private readonly IAccountService _accounts;
    private readonly IPortfolioService _portfolios;

    // Strongly-typed include sets (aligns with your "no hard-coded string" preference).
    // Adjust the IncludeOption values below to match your actual enum members.
    private static readonly IncludeOption[] NeedsHoldings = new[] { IncludeOption.Holdings };
    private static readonly IncludeOption[] NeedsTransactions = new[] { IncludeOption.Transactions };
    private static readonly IncludeOption[] NeedsAcctTx = new[] { IncludeOption.Accounts, IncludeOption.Transactions };
    private static readonly IncludeOption[] NeedsAcctHoldings = new[] { IncludeOption.Accounts, IncludeOption.Holdings };

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportingController"/> class.
    /// </summary>
    /// <param name="reporting"></param>
    /// <param name="accounts"></param>
    /// <param name="portfolios"></param>
    public ReportingController(
        IReportingService reporting,
        IAccountService accounts,
        IPortfolioService portfolios)
    {
        _reporting = reporting;
        _accounts = accounts;
        _portfolios = portfolios;
    }

    // ============================
    // Account-scoped endpoints
    // ============================

    /// <summary>Aggregate account market value by asset class for a given date (in reporting currency).</summary>
    [HttpGet("portfolios/{portfolioId:int}/accounts/{accountId:int}/asset-class-aggregate")]
    [ProducesResponseType(typeof(List<AssetClassAmountDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AggregateByAssetClassForAccount(
        [FromRoute] int portfolioId,
        [FromRoute] int accountId,
        [FromQuery][Required] DateOnly date,
        [FromQuery][Required] string currency,
        CancellationToken ct)
    {
        var accountDto = await _accounts.GetAccountWithIncludesAsync(portfolioId, accountId, NeedsHoldings, ct);
        if (accountDto is null) return NotFound();

        Currency reportingCcy = new Currency(currency);
        //if (!TryGetCurrency(currency, out var reportingCcy, out var currencyError))
        //    return ValidationProblem(currencyError);

        var account = AccountMapper.ToEntity(accountDto);
        var dict = await _reporting.AggregateByAssetClassAsync(account, date, reportingCcy!, ct);

        var result = dict.Select(kv => new AssetClassAmountDTO(kv.Key.ToString(), kv.Value.Amount, kv.Value.Currency.Code))
        .OrderByDescending(x => x.Amount)
        .ToList();
        return Ok(result);
    }

    /// <summary>Asset-class percentages for an account on a given date.</summary>
    [HttpGet("portfolios/{portfolioId:int}/accounts/{accountId:int}/asset-class-percentages")]
    [ProducesResponseType(typeof(List<AssetClassPercentageDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssetClassPercentagesForAccount(
        [FromRoute] int portfolioId,
        [FromRoute] int accountId,
        [FromQuery][Required] DateOnly date,
        [FromQuery][Required] string currency,
        CancellationToken ct)
    {
        var accountDto = await _accounts.GetAccountWithIncludesAsync(portfolioId, accountId, NeedsHoldings, ct);
        if (accountDto is null) return NotFound();

        Currency reportingCcy = new Currency(currency);
        //if (!TryGetCurrency(currency, out var reportingCcy, out var currencyError))
        //    return ValidationProblem(currencyError);

        var account = AccountMapper.ToEntity(accountDto);
        var dict = await _reporting.GetAssetClassPercentagesAsync(account, date, reportingCcy!, ct);

        var result = dict.Select(kv => new AssetClassPercentageDTO(kv.Key.ToString(), kv.Value))
        .OrderByDescending(x => x.Percentage)
        .ToList();
        return Ok(result);
    }

    /// <summary>Trading costs by currency for an account over a date range.</summary>
    [HttpGet("portfolios/{portfolioId:int}/accounts/{accountId:int}/trading-costs")]
    [ProducesResponseType(typeof(List<TradingCostsByCurrencyDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TradingCostsByCurrencyForAccount(
        [FromRoute] int portfolioId,
        [FromRoute] int accountId,
        [FromQuery][Required] DateOnly from,
        [FromQuery][Required] DateOnly to,
        CancellationToken ct)
    {
        if (from > to) return ValidationProblem("'from' must be <= 'to'.");

        var accountDto = await _accounts.GetAccountWithIncludesAsync(portfolioId, accountId, NeedsTransactions, ct);
        if (accountDto is null) return NotFound();

        var account = AccountMapper.ToEntity(accountDto);
        var dict = _reporting.GetTradingCostsByCurrency(account, from, to);

        var result = dict.Select(kv => new TradingCostsByCurrencyDTO(kv.Key.Code, kv.Value))
        .OrderByDescending(x => x.TotalCosts)
        .ToList();
        return Ok(result);
    }

    /// <summary>Transaction cost summaries (buy/sell/dividend/interest) for an account.</summary>
    [HttpGet("portfolios/{portfolioId:int}/accounts/{accountId:int}/transaction-cost-summaries")]
    [ProducesResponseType(typeof(List<TransactionCostSummaryDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransactionCostSummariesForAccount(
        [FromRoute] int portfolioId,
        [FromRoute] int accountId,
        [FromQuery][Required] DateOnly from,
        [FromQuery][Required] DateOnly to,
        CancellationToken ct)
    {
        if (from > to) return ValidationProblem("'from' must be <= 'to'.");

        var accountDto = await _accounts.GetAccountWithIncludesAsync(portfolioId, accountId, NeedsTransactions, ct);
        if (accountDto is null) return NotFound();

        var account = AccountMapper.ToEntity(accountDto);
        var summaries = _reporting.GetTransactionCostSummaries(account, from, to)
        .Select(TransactionCostSummaryMapper.FromEntity)
        .ToList();
        return Ok(summaries);
    }

    /// <summary>Transaction costs by security for an account.</summary>
    [HttpGet("portfolios/{portfolioId:int}/accounts/{accountId:int}/transaction-costs-by-security")]
    [ProducesResponseType(typeof(List<TradingCostsByCurrencyDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransactionCostsBySecurityForAccount(
        [FromRoute] int portfolioId,
        [FromRoute] int accountId,
        [FromQuery][Required] DateOnly from,
        [FromQuery][Required] DateOnly to,
        CancellationToken ct)
    {
        if (from > to) return ValidationProblem("'from' must be <= 'to'.");

        var accountDto = await _accounts.GetAccountWithIncludesAsync(portfolioId, accountId, NeedsTransactions, ct);
        if (accountDto is null) return NotFound();

        var account = AccountMapper.ToEntity(accountDto);
        var rows = _reporting.GetTransactionCostsBySecurity(account, from, to)
        .Select(r => new TransactionCostsBySecurityDTO(
        r.Symbol,
        r.Currency.Code,
        r.TotalCosts,
        r.Gross,
        r.Type.ToString()))
        .OrderByDescending(x => x.TotalCosts)
        .ToList();
        return Ok(rows);
    }

    // ============================
    // Portfolio-scoped endpoints
    // ============================

    /// <summary>Aggregate portfolio market value by asset class for a given date (in reporting currency).</summary>
    [HttpGet("portfolios/{portfolioId:int}/asset-class-aggregate")]
    [ProducesResponseType(typeof(List<AssetClassAmountDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AggregateByAssetClassForPortfolio(
        [FromRoute] int portfolioId,
        [FromQuery][Required] DateOnly date,
        [FromQuery][Required] string currency,
        CancellationToken ct)
    {
        var portfolioDto = await _portfolios.GetByIdWithIncludesAsync(portfolioId, NeedsAcctHoldings, ct);
        if (portfolioDto is null) return NotFound();

        Currency reportingCcy = new Currency(currency);
        //if (!TryGetCurrency(currency, out var reportingCcy, out var currencyError))
        //    return ValidationProblem(currencyError);

        var portfolio = PortfolioMapper.ToEntity(portfolioDto);
        var dict = await _reporting.AggregateByAssetClassAsync(portfolio, date, reportingCcy!, ct);

        var result = dict.Select(kv => new AssetClassAmountDTO(kv.Key.ToString(), kv.Value.Amount, kv.Value.Currency.Code))
        .OrderByDescending(x => x.Amount)
        .ToList();
        return Ok(result);
    }

    /// <summary>Asset-class percentages for a portfolio on a given date.</summary>
    [HttpGet("portfolios/{portfolioId:int}/asset-class-percentages")]
    [ProducesResponseType(typeof(List<AssetClassPercentageDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssetClassPercentagesForPortfolio(
        [FromRoute] int portfolioId,
        [FromQuery][Required] DateOnly date,
        [FromQuery][Required] string currency,
        CancellationToken ct)
    {
        var portfolioDto = await _portfolios.GetByIdWithIncludesAsync(portfolioId, NeedsAcctHoldings, ct);
        if (portfolioDto is null) return NotFound();

        Currency reportingCcy = new Currency(currency);
        //if (!TryGetCurrency(currency, out var reportingCcy, out var currencyError))
        //    return ValidationProblem(currencyError);

        var portfolio = PortfolioMapper.ToEntity(portfolioDto);
        var dict = await _reporting.GetAssetClassPercentagesAsync(portfolio, date, reportingCcy!, ct);

        var result = dict.Select(kv => new AssetClassPercentageDTO(kv.Key.ToString(), kv.Value))
        .OrderByDescending(x => x.Percentage)
        .ToList();
        return Ok(result);
    }

    /// <summary>Trading costs by currency for a portfolio over a date range.</summary>
    [HttpGet("portfolios/{portfolioId:int}/trading-costs")]
    [ProducesResponseType(typeof(List<TradingCostsByCurrencyDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TradingCostsByCurrencyForPortfolio(
        [FromRoute] int portfolioId,
        [FromQuery][Required] DateOnly from,
        [FromQuery][Required] DateOnly to,
        CancellationToken ct)
    {
        if (from > to) return ValidationProblem("'from' must be <= 'to'.");

        var portfolioDto = await _portfolios.GetByIdWithIncludesAsync(portfolioId, NeedsAcctTx, ct);
        if (portfolioDto is null) return NotFound();

        var portfolio = PortfolioMapper.ToEntity(portfolioDto);
        var dict = _reporting.GetTradingCostsByCurrency(portfolio, from, to);

        var result = dict.Select(kv => new TradingCostsByCurrencyDTO(kv.Key.Code, kv.Value))
        .OrderByDescending(x => x.TotalCosts)
        .ToList();
        return Ok(result);
    }

    /// <summary>Transaction cost summaries for a portfolio.</summary>
    [HttpGet("portfolios/{portfolioId:int}/transaction-cost-summaries")]
    [ProducesResponseType(typeof(List<TransactionCostSummaryDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransactionCostSummariesForPortfolio(
        [FromRoute] int portfolioId,
        [FromQuery][Required] DateOnly from,
        [FromQuery][Required] DateOnly to,
        CancellationToken ct)
    {
        if (from > to) return ValidationProblem("'from' must be <= 'to'.");

        var portfolioDto = await _portfolios.GetByIdWithIncludesAsync(portfolioId, NeedsAcctTx, ct);
        if (portfolioDto is null) return NotFound();

        var portfolio = PortfolioMapper.ToEntity(portfolioDto);
        var summaries = _reporting.GetTransactionCostSummaries(portfolio, from, to)
        .Select(TransactionCostSummaryMapper.FromEntity)
        .ToList();
        return Ok(summaries);
    }

    /// <summary>Transaction costs by security for a portfolio.</summary>
    [HttpGet("portfolios/{portfolioId:int}/transaction-costs-by-security")]
    [ProducesResponseType(typeof(List<TradingCostsByCurrencyDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransactionCostsBySecurityForPortfolio(
        [FromRoute] int portfolioId,
        [FromQuery][Required] DateOnly from,
        [FromQuery][Required] DateOnly to,
        CancellationToken ct)
    {
        if (from > to) return ValidationProblem("'from' must be <= 'to'.");

        var portfolioDto = await _portfolios.GetByIdWithIncludesAsync(portfolioId, NeedsAcctTx, ct);
        if (portfolioDto is null) return NotFound();

        var portfolio = PortfolioMapper.ToEntity(portfolioDto);
        var rows = _reporting.GetTransactionCostsBySecurity(portfolio, from, to)
        .Select(r => new TransactionCostsBySecurityDTO(
            r.Symbol,
            r.Currency.Code,
            r.TotalCosts,
            r.Gross,
            r.Type.ToString()))
        .OrderByDescending(x => x.TotalCosts)
        .ToList();
        return Ok(rows);
    }
    /*    private bool TryGetCurrency(string code, out Currency? currency, out string? error)
       {
           currency = null;
           error = null;

           if (string.IsNullOrWhiteSpace(code))
           {
               error = "Query parameter 'currency' is required (e.g., CAD, USD).";
               return false;
           }

           try
           {
               // Replace if you use a different factory/registry
               currency = new Currency(code);
               return true;
           }
           catch
           {
               error = $"Unsupported currency code '{code}'.";
               return false;
           }
       } */
}
