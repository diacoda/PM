using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Mappers;
using PM.DTO;

namespace PM.API.Controllers;

/// <summary>
/// Manages investment portfolios and their related accounts.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PortfoliosController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;
    private readonly IAccountService _accountService;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="portfolioService"></param>
    /// <param name="accountService"></param>
    public PortfoliosController(IPortfolioService portfolioService, IAccountService accountService)
    {
        _portfolioService = portfolioService;
        _accountService = accountService;
    }

    /// <summary>
    /// Creates a new investment portfolio for the specified owner.
    /// </summary>
    /// <param name="dto">The portfolio creation details including the owner name.</param>
    /// <returns>The newly created portfolio.</returns>
    /// <response code="200">Portfolio created successfully.</response>
    /// <response code="400">Invalid input data.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PortfolioDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePortfolioDTO dto)
    {
        var createdPortfolio = await _portfolioService.CreateAsync(dto.Owner);
        return Ok(PortfolioMapper.ToDTO(createdPortfolio));
    }

    /// <summary>
    /// Adds a new account to an existing portfolio.
    /// </summary>
    /// <param name="portfolioId">The ID of the portfolio to which the account will be added.</param>
    /// <param name="dto">The account creation details including name, currency, and institution.</param>
    /// <returns>The created account added to the portfolio.</returns>
    /// <response code="200">Account created and added to portfolio successfully.</response>
    /// <response code="404">Portfolio not found.</response>
    /// <response code="400">Invalid input data.</response>
    [HttpPost("{portfolioId}/accounts")]
    [ProducesResponseType(typeof(AccountDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddAccountAsync(int portfolioId, [FromBody] CreateAccountDTO dto)
    {
        /*
        var requestAccount = AccountMapper.ToEntity(dto);
        var createdAccount = await _accountService.CreateAsync(
            requestAccount.Name,
            requestAccount.Currency,
            requestAccount.FinancialInstitution);

        var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
        if (portfolio == null) return NotFound();

        await _accountService.AddAccountToPortfolioAsync(portfolio.Id, createdAccount.Id);
        return Ok(createdAccount);
        */
        var requestAccount = AccountMapper.ToEntity(dto);
        var account = await _accountService.CreateAsync(
                portfolioId,
                requestAccount.Name,
                requestAccount.Currency,
                requestAccount.FinancialInstitution);
        return Ok(AccountMapper.ToDTO(account));

    }

    /// <summary>
    /// Lists all accounts associated with a specific portfolio.
    /// </summary>
    /// <param name="portfolioId">The ID of the portfolio whose accounts will be listed.</param>
    /// <returns>A list of accounts under the given portfolio.</returns>
    /// <response code="200">List of accounts returned successfully.</response>
    /// <response code="404">Portfolio not found.</response>
    [HttpGet("{portfolioId}/accounts")]
    [ProducesResponseType(typeof(IEnumerable<Account>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListAccounts(int portfolioId)
    {
        var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
        if (portfolio == null) return NotFound();

        return Ok(await _accountService.ListAccountsAsync(portfolio.Id));
    }

    /// <summary>
    /// Retrieves details of a specific portfolio.
    /// </summary>
    /// <param name="portfolioId">The ID of the portfolio to retrieve.</param>
    /// <returns>The portfolio details.</returns>
    /// <response code="200">Portfolio details returned successfully.</response>
    /// <response code="404">Portfolio not found.</response>
    [HttpGet("{portfolioId}")]
    [ProducesResponseType(typeof(PortfolioDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int portfolioId)
    {
        var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
        if (portfolio == null) return NotFound();
        return Ok(PortfolioMapper.ToDTO(portfolio));
    }

    /// <summary>
    /// Lists all existing portfolios.
    /// </summary>
    /// <returns>A list of all portfolios.</returns>
    /// <response code="200">List of portfolios returned successfully.</response>
    /// <response code="404">No portfolios found.</response>
    [HttpGet("")]
    [ProducesResponseType(typeof(IEnumerable<PortfolioDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List()
    {
        var portfolios = await _portfolioService.ListAsync();
        if (portfolios == null || !portfolios.Any())
            return NotFound();

        var portfolioDtos = portfolios.Select(PortfolioMapper.ToDTO);
        return Ok(portfolioDtos);
    }
    /// <summary>
    /// Deletes an account from a portfolio.
    /// </summary>
    /// <param name="portfolioId">The ID of the portfolio.</param>
    /// <param name="accountId">The ID of the account to delete.</param>
    /// <returns>No content if successful; 404 if not found or not linked.</returns>
    [HttpDelete("{portfolioId}/accounts/{accountId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAccountFromPortfolio(int portfolioId, int accountId)
    {
        try
        {
            await _accountService.RemoveAccountFromPortfolioAsync(portfolioId, accountId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a specific account from a portfolio.
    /// </summary>
    /// <param name="portfolioId">The ID of the portfolio.</param>
    /// <param name="accountId">The ID of the account to retrieve.</param>
    /// <returns>The account details if found.</returns>
    [HttpGet("{portfolioId}/accounts/{accountId}")]
    [ProducesResponseType(typeof(AccountDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAccountFromPortfolio(int portfolioId, int accountId)
    {
        try
        {
            var account = await _accountService.GetAccountAsync(portfolioId, accountId);
            if (account is null)
                return NotFound(new ProblemDetails { Title = $"Account {accountId} not found in Portfolio {portfolioId}." });

            return Ok(AccountMapper.ToDTO(account));
        }
        catch (Exception ex)
        {
            return BadRequest(new ProblemDetails { Title = "An unexpected error occurred", Detail = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a portfolio by ID.
    /// </summary>
    /// <param name="portfolioId">The ID of the portfolio to delete.</param>
    /// <returns>No content if successful; 404 if not found.</returns>
    [HttpDelete("{portfolioId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePortfolio(int portfolioId)
    {
        try
        {
            await _portfolioService.DeleteAsync(portfolioId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message });
        }
    }

}