using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.DTO;
using PM.Domain.Values;
using PM.Domain.Mappers;

namespace PM.API.Controllers
{
    /// <summary>
    /// Controller for managing holdings within accounts in a portfolio.
    /// Supports listing, retrieving, creating, updating, and deleting holdings.
    /// </summary>
    [ApiController]
    [Route("api/portfolios/{portfolioId}/accounts/{accountId}/holdings")]
    [Produces("application/json")]
    public class HoldingsController : ControllerBase
    {
        private readonly IHoldingService _holdingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HoldingsController"/> class.
        /// </summary>
        /// <param name="holdingService">Service to handle holding operations.</param>
        public HoldingsController(IHoldingService holdingService)
        {
            _holdingService = holdingService;
        }

        /// <summary>
        /// Lists all holdings for the specified account.
        /// </summary>
        /// <param name="accountId">The account ID to list holdings for.</param>
        /// <param name="ct">Cancellation token for the request.</param>
        /// <returns>Returns 200 OK with a list of holdings.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<HoldingDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List(int accountId, CancellationToken ct = default)
        {
            var holdings = await _holdingService.ListHoldingsAsync(accountId, ct);
            return Ok(holdings.Select(HoldingMapper.ToDTO));
        }

        /// <summary>
        /// Retrieves a specific holding by symbol.
        /// </summary>
        /// <param name="accountId">The account ID containing the holding.</param>
        /// <param name="symbol">The symbol of the holding.</param>
        /// <param name="ct">Cancellation token for the request.</param>
        /// <returns>Returns 200 OK with the holding, or 404 if not found.</returns>
        [HttpGet("{symbol}")]
        [ProducesResponseType(typeof(HoldingDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int accountId, string symbol, CancellationToken ct = default)
        {
            var holding = await _holdingService.GetHoldingAsync(accountId, new Symbol(symbol), ct);
            if (holding is null) return NotFound();
            return Ok(HoldingMapper.ToDTO(holding));
        }

        /// <summary>
        /// Creates or updates a holding for the specified account.
        /// </summary>
        /// <param name="accountId">The account ID to add or update the holding for.</param>
        /// <param name="symbol">The symbol of the holding (from route).</param>
        /// <param name="dto">The holding data, e.g., quantity.</param>
        /// <param name="ct">Cancellation token for the request.</param>
        /// <returns>Returns 200 OK with the created or updated holding.</returns>
        [HttpPatch("{symbol}")]
        [ProducesResponseType(typeof(HoldingDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Upsert(
            int accountId,
            [FromRoute] string symbol,
            [FromBody] ModifyHoldingDTO dto,
            CancellationToken ct = default)
        {
            var s = new Symbol(symbol);
            await _holdingService.UpsertHoldingAsync(accountId, s, dto.Quantity, ct);

            var holding = await _holdingService.GetHoldingAsync(accountId, s, ct);
            return Ok(HoldingMapper.ToDTO(holding!));
        }

        /// <summary>
        /// Deletes a holding from the specified account.
        /// </summary>
        /// <param name="accountId">The account ID containing the holding.</param>
        /// <param name="symbol">The symbol of the holding to delete.</param>
        /// <param name="ct">Cancellation token for the request.</param>
        /// <returns>
        /// Returns 204 No Content if deleted successfully, or 404 if the holding does not exist.
        /// </returns>
        [HttpDelete("{symbol}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int accountId, string symbol, CancellationToken ct = default)
        {
            var holding = await _holdingService.GetHoldingAsync(accountId, new Symbol(symbol), ct);
            if (holding is null) return NotFound();

            await _holdingService.RemoveHoldingAsync(accountId, new Symbol(symbol), ct);
            return NoContent();
        }

        /// <summary>
        /// Updates the quantity of an existing holding.
        /// </summary>
        /// <param name="accountId">The account ID containing the holding.</param>
        /// <param name="symbol">The symbol of the holding to update.</param>
        /// <param name="quantity">The new holding data including updated quantity.</param>
        /// <param name="ct">Cancellation token for the request.</param>
        /// <returns>
        /// Returns 200 OK with the updated holding, or 404 if the holding does not exist.
        /// </returns>
        [HttpPut("{symbol}")]
        [ProducesResponseType(typeof(HoldingDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            int accountId,
            string symbol,
            [FromBody] ModifyHoldingDTO quantity,
            CancellationToken ct = default)
        {
            Symbol s = new Symbol(symbol);
            var holding = await _holdingService.GetHoldingAsync(accountId, s, ct);
            if (holding is null) return NotFound();

            await _holdingService.UpdateHoldingQuantityAsync(accountId, s, quantity.Quantity, ct);
            return Ok(HoldingMapper.ToDTO(holding));
        }
    }
}
