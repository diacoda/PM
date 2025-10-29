using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.DTO;

namespace PM.API.Controllers;

/// <summary>
/// Handles cash flow operations such as deposits, withdrawals, and fees for investment accounts.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CashFlowsController : ControllerBase
{
    private readonly IAccountManager _accountManager;
    private readonly IAccountService _accountService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CashFlowsController"/> class.
    /// </summary>
    /// <param name="accountManager">Service responsible for performing cash flow operations.</param>
    /// <param name="accountService">Service responsible for retrieving account information.</param>
    public CashFlowsController(IAccountManager accountManager, IAccountService accountService)
    {
        _accountManager = accountManager;
        _accountService = accountService;
    }

    /// <summary>
    /// Deposits cash into a specified account.
    /// </summary>
    /// <param name="dto">Details of the deposit including portfolio ID, account ID, amount, currency, date, and note.</param>
    /// <returns>
    /// Returns 200 OK if the deposit is successful.
    /// Returns 400 Bad Request if the portfolio or account is invalid.
    /// </returns>
    [HttpPost("deposit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deposit([FromBody] CashFlowDTO dto)
    {
        var account = await _accountService.GetAccountAsync(dto.PortfolioId, dto.AccountId);
        if (account is null)
            return BadRequest(new ProblemDetails { Title = "Invalid portfolio/account" });

        await _accountManager.Deposit(account, dto.Amount, dto.Currency, dto.Date, dto.Note);
        return Ok();
    }

    /// <summary>
    /// Withdraws cash from a specified account.
    /// </summary>
    /// <param name="dto">Details of the withdrawal including portfolio ID, account ID, amount, currency, date, and note.</param>
    /// <returns>
    /// Returns 200 OK if the withdrawal is successful.
    /// Returns 400 Bad Request if the portfolio or account is invalid.
    /// </returns>
    [HttpPost("withdraw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Withdraw([FromBody] CashFlowDTO dto)
    {
        var account = await _accountService.GetAccountAsync(dto.PortfolioId, dto.AccountId);
        if (account is null)
            return BadRequest(new ProblemDetails { Title = "Invalid portfolio/account" });

        await _accountManager.Withdraw(account, dto.Amount, dto.Currency, dto.Date, dto.Note);
        return Ok();
    }

    /// <summary>
    /// Applies a fee to a specified account.
    /// </summary>
    /// <param name="dto">Details of the fee including portfolio ID, account ID, amount, currency, date, and note.</param>
    /// <returns>
    /// Returns 200 OK if the fee is successfully applied.
    /// Returns 400 Bad Request if the portfolio or account is invalid.
    /// </returns>
    [HttpPost("fee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Fee([FromBody] CashFlowDTO dto)
    {
        var account = await _accountService.GetAccountAsync(dto.PortfolioId, dto.AccountId);
        if (account is null)
            return BadRequest(new ProblemDetails { Title = "Invalid portfolio/account" });

        await _accountManager.Fee(account, dto.Amount, dto.Currency, dto.Date, dto.Note);
        return Ok();
    }
}
