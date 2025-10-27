using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PM.Application.Services;
using PM.Domain.Mappers;
using PM.Domain.Values;
using PM.DTO;

namespace PM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfoliosController : ControllerBase
{
    private readonly PortfolioService _portfolioService;
    private readonly AccountService _accountService;

    public PortfoliosController(PortfolioService portfolioService, AccountService accountService)
    {
        _portfolioService = portfolioService;
        _accountService = accountService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePortfolioDTO dto)
    {
        var createdPortfolio = await _portfolioService.CreateAsync(dto.Owner);
        return Ok(createdPortfolio);
    }

    [HttpPost("{portfolioId}/accounts")]
    public async Task<IActionResult> AddAccountAsync(int portfolioId, [FromBody] CreateAccountDTO dto)
    {
        var requestAccount = AccountMapper.ToEntity(dto);
        var createdAccount = await _accountService.CreateAsync(requestAccount.Name, requestAccount.Currency, requestAccount.FinancialInstitution);
        var portfolio = _portfolioService.GetByIdAsync(portfolioId);
        if (portfolio == null) return NotFound();
        await _accountService.AddAccountToPortfolioAsync(portfolio.Id, createdAccount.Id);
        return Ok(createdAccount);
    }

    [HttpGet("{portfolioId}/accounts")]
    public async Task<IActionResult> ListAccounts(int portfolioId)
    {
        var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
        if (portfolio == null) return NotFound();
        return Ok(await _accountService.ListAccountsAsync(portfolio.Id));
    }
}
