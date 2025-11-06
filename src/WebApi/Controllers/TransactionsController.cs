using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.DTO;
using PM.Domain.Mappers;
using PM.Domain.Enums;

namespace PM.API.Controllers
{
    /// <summary>
    /// Controller responsible for managing transactions within a portfolio account.
    /// Supports creating, retrieving, and listing transactions such as deposits, withdrawals, and trades.
    /// </summary>
    [ApiController]
    [Route("api/portfolios/{portfolioId}/accounts/{accountId}/transactions")]
    [Produces("application/json")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ITransactionWorkflowService _workflow;
        private readonly IAccountService _accountService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionsController"/> class.
        /// </summary>
        /// <param name="transactionService">Service for managing transaction persistence.</param>
        /// <param name="workflowService">Service for orchestrating transaction processing logic.</param>
        /// <param name="accountService">Service for retrieving and validating portfolio accounts.</param>
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
        /// Records a cash deposit into the specified account.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio containing the account.</param>
        /// <param name="accountId">The ID of the account receiving the deposit.</param>
        /// <param name="dto">The deposit details, including amount, currency, date, and optional note.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The completed deposit as a <see cref="TransactionDTO"/>.</returns>
        /// <response code="200">Deposit recorded successfully.</response>
        /// <response code="400">Invalid portfolio or account.</response>
        [HttpPost("deposit")]
        [ProducesResponseType(typeof(TransactionDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TransactionDTO>> Deposit(
            [FromRoute] int portfolioId,
            [FromRoute] int accountId,
            [FromBody] CashFlowDTO dto,
            CancellationToken ct)
        {
            var account = await _accountService.GetAccountAsync(portfolioId, accountId, ct);
            if (account is null)
                return BadRequest(new ProblemDetails { Title = "Invalid portfolio/account" });

            var tx = TransactionMapper.ToEntity(accountId, TransactionType.Deposit, dto);
            var result = await _workflow.ProcessTransactionAsync(tx, ct);
            return Ok(TransactionMapper.ToDTO(result));
        }

        /// <summary>
        /// Records a cash withdrawal from the specified account.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio containing the account.</param>
        /// <param name="accountId">The ID of the account from which funds will be withdrawn.</param>
        /// <param name="dto">The withdrawal details, including amount, currency, date, and optional note.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The completed withdrawal as a <see cref="TransactionDTO"/>.</returns>
        /// <response code="200">Withdrawal recorded successfully.</response>
        /// <response code="400">Invalid portfolio or account.</response>
        [HttpPost("withdraw")]
        [ProducesResponseType(typeof(TransactionDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TransactionDTO>> Withdraw(
            [FromRoute] int portfolioId,
            [FromRoute] int accountId,
            [FromBody] CashFlowDTO dto,
            CancellationToken ct)
        {
            var account = await _accountService.GetAccountAsync(portfolioId, accountId, ct);
            if (account is null)
                return BadRequest(new ProblemDetails { Title = "Invalid portfolio/account" });

            var tx = TransactionMapper.ToEntity(accountId, TransactionType.Withdrawal, dto);
            var result = await _workflow.ProcessTransactionAsync(tx, ct);
            return Ok(result);
        }

        /// <summary>
        /// Creates a general transaction (e.g., Buy, Sell, Dividend) for the specified account.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio containing the account.</param>
        /// <param name="accountId">The ID of the account for which the transaction is created.</param>
        /// <param name="dto">The transaction details including type, symbol, quantity, amount, currency, and date.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The created <see cref="TransactionDTO"/>.</returns>
        /// <response code="200">Transaction created successfully.</response>
        /// <response code="400">Invalid portfolio or account.</response>
        [HttpPost]
        [ProducesResponseType(typeof(TransactionDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TransactionDTO>> CreateTransaction(
            [FromRoute] int portfolioId,
            [FromRoute] int accountId,
            [FromBody] CreateTransactionDTO dto,
            CancellationToken ct)
        {
            var account = await _accountService.GetAccountAsync(portfolioId, accountId, ct);
            if (account is null)
                return BadRequest(new ProblemDetails { Title = "Invalid portfolio/account" });

            var tx = TransactionMapper.ToEntity(accountId, dto);
            var result = await _workflow.ProcessTransactionAsync(tx, ct);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a list of all transactions for the specified account.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio containing the account.</param>
        /// <param name="accountId">The ID of the account whose transactions will be listed.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A collection of <see cref="TransactionDTO"/> representing account transactions.</returns>
        /// <response code="200">Returns a list of transactions for the account.</response>
        /// <response code="400">Invalid portfolio or account.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TransactionDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetList(
            [FromRoute] int portfolioId,
            [FromRoute] int accountId,
            CancellationToken ct)
        {
            var account = await _accountService.GetAccountAsync(portfolioId, accountId, ct);
            if (account is null)
                return BadRequest(new ProblemDetails { Title = "Invalid portfolio/account" });

            var transactions = await _transactionService.ListTransactionsAsync(accountId, ct);
            var dtoList = transactions.Select(TransactionMapper.ToDTO);
            return Ok(dtoList);
        }

        /// <summary>
        /// Retrieves a single transaction by ID for the specified account.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio containing the account.</param>
        /// <param name="accountId">The ID of the account containing the transaction.</param>
        /// <param name="transactionId">The ID of the transaction to retrieve.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The requested <see cref="TransactionDTO"/> if found.</returns>
        /// <response code="200">Transaction found and returned successfully.</response>
        /// <response code="400">Invalid portfolio or account.</response>
        /// <response code="404">Transaction not found.</response>
        [HttpGet("{transactionId}")]
        [ProducesResponseType(typeof(TransactionDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(
            [FromRoute] int portfolioId,
            [FromRoute] int accountId,
            int transactionId,
            CancellationToken ct)
        {
            var account = await _accountService.GetAccountAsync(portfolioId, accountId, ct);
            if (account is null)
                return BadRequest(new ProblemDetails { Title = "Invalid portfolio/account" });

            var tx = await _transactionService.GetTransactionAsync(accountId, transactionId, ct);
            if (tx is null)
                return NotFound(new ProblemDetails { Title = "Transaction not found" });

            return Ok(TransactionMapper.ToDTO(tx));
        }
    }
}
