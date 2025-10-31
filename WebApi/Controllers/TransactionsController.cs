using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.DTO;
using PM.Domain.Mappers;

namespace PM.API.Controllers;

[ApiController]
[Route("api/portfolios/{portfolioId}/accounts/{accountId}/transactions")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ITransactionWorkflowService _workflow;
    private readonly IAccountService _accountService;

    public TransactionsController(
        ITransactionService transactionService,
        ITransactionWorkflowService workflowService,
        IAccountService accountService)
    {
        _transactionService = transactionService;
        _workflow = workflowService;
        _accountService = accountService;
    }

    /// <summary>
    /// Deposits cash into a specified account.
    /// </summary>
    /// <param name="portfolioId">Portfolio ID</param>
    /// <param name="accountId">Account ID</param>
    /// <param name="dto">Details of the deposit including portfolio ID, account ID, amount, currency, date, and note.</param>
    /// <returns>
    /// Returns 200 OK if the deposit is successful.
    /// Returns 400 Bad Request if the portfolio or account is invalid.
    /// </returns>
    [HttpPost()]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransactionDTO>> CreateTransaction(
        [FromRoute] int portfolioId,
        [FromRoute] int accountId,
        [FromBody] CreateTransactionDTO dto, CancellationToken ct)
    {
        var account = await _accountService.GetAccountAsync(portfolioId, accountId);
        if (account is null)
            return BadRequest(new ProblemDetails { Title = "Invalid portfolio/account" });

        var tx = TransactionMapper.ToEntity(accountId, dto);
        var result = await _workflow.ProcessTransactionAsync(tx, ct);
        return Ok(TransactionMapper.ToDTO(result));
    }

    [HttpGet()]
    public async Task<IActionResult> GetList(
        [FromRoute] int portfolioId,
        [FromRoute] int accountId
    )
    {
        var account = await _accountService.GetAccountAsync(portfolioId, accountId);
        if (account is null)
            return BadRequest(new ProblemDetails { Title = "Invalid portfolio/account" });

        await _transactionService.ListTransactionsAsync(accountId);
        return Ok();
    }

    [HttpGet("{transactionId}")]
    public async Task<IActionResult> Get(
        [FromRoute] int portfolioId,
        [FromRoute] int accountId,
        int transactionId
    )
    {
        var account = await _accountService.GetAccountAsync(portfolioId, accountId);
        if (account is null)
            return BadRequest(new ProblemDetails { Title = "Invalid portfolio/account" });

        return Ok();
    }
}
