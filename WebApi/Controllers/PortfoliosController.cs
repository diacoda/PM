using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.Domain.Mappers;
using PM.DTO;
using PM.SharedKernel;

namespace PM.API.Controllers
{
    /// <summary>
    /// Manages investment portfolios and their related accounts.
    /// Supports flexible includes for nested data like accounts, holdings, and transactions.
    /// </summary>
    [ApiController]
    [Route("api/portfolios")]
    [Produces("application/json")]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;
        private readonly IAccountService _accountService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortfolioController"/> class.
        /// </summary>
        public PortfolioController(IPortfolioService portfolioService, IAccountService accountService)
        {
            _portfolioService = portfolioService;
            _accountService = accountService;
        }

        /// <summary>
        /// Retrieves all portfolios.
        /// </summary>
        /// <param name="includes">Optional nested data to include (e.g., accounts, holdings).</param>
        /// <param name="ct">Cancellation token for the request.</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PortfolioDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery(Name = "include")] IncludeOption[] includes, CancellationToken ct = default)
        {
            var result = includes.Length == 0
                ? await _portfolioService.ListAsync(ct)
                : await _portfolioService.ListWithIncludesAsync(includes, ct);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific portfolio by ID.
        /// </summary>
        /// <param name="portfolioId">Portfolio ID to retrieve.</param>
        /// <param name="includes">Optional nested data to include.</param>
        /// <param name="ct">Cancellation token for the request.</param>
        [HttpGet("{portfolioId}")]
        [ProducesResponseType(typeof(PortfolioDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int portfolioId, [FromQuery(Name = "include")] IncludeOption[] includes, CancellationToken ct = default)
        {
            var dto = includes.Length == 0
                ? await _portfolioService.GetByIdAsync(portfolioId, ct)
                : await _portfolioService.GetByIdWithIncludesAsync(portfolioId, includes, ct);

            return dto is null
                ? NotFound(new ProblemDetails { Title = $"Portfolio {portfolioId} not found." })
                : Ok(dto);
        }

        /// <summary>
        /// Retrieves all accounts for a portfolio.
        /// </summary>
        /// <param name="portfolioId">Portfolio ID to retrieve accounts for.</param>
        /// <param name="includes">Optional nested data to include in accounts.</param>
        /// <param name="ct">Cancellation token for the request.</param>
        [HttpGet("{portfolioId}/accounts")]
        [ProducesResponseType(typeof(IEnumerable<AccountDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListAccounts(int portfolioId, [FromQuery(Name = "include")] IncludeOption[] includes, CancellationToken ct = default)
        {
            var result = includes.Length == 0
                ? await _accountService.ListAccountsAsync(portfolioId, ct)
                : await _accountService.ListAccountsWithIncludesAsync(portfolioId, includes, ct);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific account from a portfolio.
        /// </summary>
        /// <param name="portfolioId">Portfolio ID containing the account.</param>
        /// <param name="accountId">Account ID to retrieve.</param>
        /// <param name="includes">Optional nested data to include in the account.</param>
        /// <param name="ct">Cancellation token for the request.</param>
        [HttpGet("{portfolioId}/accounts/{accountId}")]
        [ProducesResponseType(typeof(AccountDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAccount(int portfolioId, int accountId, [FromQuery(Name = "include")] IncludeOption[] includes, CancellationToken ct = default)
        {
            var dto = includes.Length == 0
                ? await _accountService.GetAccountAsync(portfolioId, accountId, ct)
                : await _accountService.GetAccountWithIncludesAsync(portfolioId, accountId, includes, ct);

            return dto is null
                ? NotFound(new ProblemDetails { Title = $"Account {accountId} not found in Portfolio {portfolioId}." })
                : Ok(dto);
        }

        /// <summary>
        /// Creates a new portfolio.
        /// </summary>
        /// <param name="dto">Portfolio creation data.</param>
        /// <param name="ct">Cancellation token for the request.</param>
        [HttpPost]
        [ProducesResponseType(typeof(PortfolioDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePortfolio([FromBody] CreatePortfolioDTO dto, CancellationToken ct = default)
        {
            var created = await _portfolioService.CreateAsync(dto.Owner, ct);
            return CreatedAtAction(nameof(Get), new { portfolioId = created.Id }, created);
        }

        /// <summary>
        /// Adds a new account to a portfolio.
        /// </summary>
        /// <param name="portfolioId">Portfolio ID to add the account to.</param>
        /// <param name="dto">Account creation data.</param>
        /// <param name="ct">Cancellation token for the request.</param>
        [HttpPost("{portfolioId}/accounts")]
        [ProducesResponseType(typeof(AccountDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddAccount(int portfolioId, [FromBody] CreateAccountDTO dto, CancellationToken ct = default)
        {
            var requested = AccountMapper.ToEntity(dto);
            var account = await _accountService.CreateAsync(portfolioId, requested.Name, requested.Currency, requested.FinancialInstitution, ct);
            return CreatedAtAction(nameof(GetAccount), new { portfolioId, accountId = account.Id }, account);
        }

        /// <summary>
        /// Removes an account from a portfolio.
        /// </summary>
        /// <param name="portfolioId">Portfolio ID containing the account.</param>
        /// <param name="accountId">Account ID to remove.</param>
        /// <param name="ct">Cancellation token for the request.</param>
        [HttpDelete("{portfolioId}/accounts/{accountId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveAccount(int portfolioId, int accountId, CancellationToken ct = default)
        {
            await _accountService.RemoveAccountFromPortfolioAsync(portfolioId, accountId, ct);
            return NoContent();
        }

        /// <summary>
        /// Deletes a portfolio.
        /// </summary>
        /// <param name="portfolioId">Portfolio ID to delete.</param>
        /// <param name="ct">Cancellation token for the request.</param>
        [HttpDelete("{portfolioId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePortfolio(int portfolioId, CancellationToken ct = default)
        {
            await _portfolioService.DeleteAsync(portfolioId, ct);
            return NoContent();
        }

        /// <summary>
        /// Updates the owner of a portfolio.
        /// </summary>
        /// <param name="portfolioId">Portfolio ID to update.</param>
        /// <param name="newOwner">New owner name.</param>
        /// <param name="ct">Cancellation token for the request.</param>
        [HttpPut("{portfolioId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateOwner(int portfolioId, [FromBody] string newOwner, CancellationToken ct = default)
        {
            await _portfolioService.UpdateOwnerAsync(portfolioId, newOwner, ct);
            return NoContent();
        }

        /// <summary>
        /// Updates account properties.
        /// </summary>
        /// <param name="portfolioId">Portfolio ID containing the account.</param>
        /// <param name="accountId">Account ID to update.</param>
        /// <param name="dto">Account update data.</param>
        /// <param name="ct">Cancellation token for the request.</param>
        [HttpPut("{portfolioId}/accounts/{accountId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAccount(int portfolioId, int accountId, [FromBody] CreateAccountDTO dto, CancellationToken ct = default)
        {
            var account = await _accountService.GetAccountAsync(portfolioId, accountId, ct);
            if (account is null)
                return NotFound(new ProblemDetails { Title = $"Account {accountId} not found in Portfolio {portfolioId}." });

            await _accountService.UpdateAccountNameAsync(accountId, dto.Name, ct);
            return NoContent();
        }
    }
}
