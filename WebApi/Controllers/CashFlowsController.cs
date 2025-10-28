using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.DTO;

namespace PM.API.Controllers;

[ApiController]
//[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
public class CashFlowsController : ControllerBase
{
    private readonly IAccountManager _accountManager;
    private readonly IAccountService _accountService;

    public CashFlowsController(IAccountManager accountManager, IAccountService accountService)
    {
        _accountManager = accountManager;
        _accountService = accountService;
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] CashFlowDTO dto)
    {
        var account = await _accountService.GetAccountAsync(dto.PortfolioId, dto.AccountId);
        if (account is null) return BadRequest(new ProblemDetails { Title = "Invalid portfolio/account" });
        await _accountManager.Deposit(account, dto.Amount, dto.Currency, dto.Date, dto.Note);
        return Ok();
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] CashFlowDTO dto)
    {
        var account = await _accountService.GetAccountAsync(dto.PortfolioId, dto.AccountId);
        if (account is null) return BadRequest(new ProblemDetails { Title = "Invalid portfolio/account" });
        await _accountManager.Withdraw(account, dto.Amount, dto.Currency, dto.Date, dto.Note);
        return Ok();
    }

    [HttpPost("fee")]
    public async Task<IActionResult> Fee([FromBody] CashFlowDTO dto)
    {
        var account = await _accountService.GetAccountAsync(dto.PortfolioId, dto.AccountId);
        if (account is null) return BadRequest(new ProblemDetails { Title = "Invalid portfolio/account" });
        await _accountManager.Fee(account, dto.Amount, dto.Currency, dto.Date, dto.Note);
        return Ok();
    }
}
