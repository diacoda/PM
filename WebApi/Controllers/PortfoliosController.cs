using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddAccountAsync(int portfolioId, [FromBody] CreateAccountDTO dto)
    {
        var requestAccount = AccountMapper.ToEntity(dto);
        var createdAccount = await _accountService.CreateAsync(requestAccount.Name, requestAccount.Currency, requestAccount.FinancialInstitution);

        var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
        if (portfolio == null) return NotFound();

        await _accountService.AddAccountToPortfolioAsync(portfolio.Id, createdAccount.Id);
        return Ok(createdAccount);
    }

    /// <summary>
    /// Lists all accounts associated with a specific portfolio.
    /// </summary>
    /// <param name="portfolioId">The ID of the portfolio whose accounts will be listed.</param>
    /// <returns>A list of accounts under the given portfolio.</returns>
    /// <response code="200">List of accounts returned successfully.</response>
    /// <response code="404">Portfolio not found.</response>
    [HttpGet("{portfolioId}/accounts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List()
    {
        var portfolios = await _portfolioService.ListAsync();
        if (portfolios == null || !portfolios.Any())
            return NotFound();

        var portfolioDtos = portfolios.Select(PortfolioMapper.ToDTO);
        return Ok(portfolioDtos);
    }
}
