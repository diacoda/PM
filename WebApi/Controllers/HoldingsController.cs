using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.DTO;
using PM.Domain.Values;
using PM.Domain.Mappers;

namespace PM.API.Controllers;

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
    /// <returns>Returns 200 OK with a list of holdings.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<HoldingDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(int accountId)
    {
        var holdings = await _holdingService.ListHoldingsAsync(accountId);
        return Ok(holdings.Select(HoldingMapper.ToDTO));
    }

    /// <summary>
    /// Retrieves a specific holding by symbol.
    /// </summary>
    /// <param name="accountId">The account ID containing the holding.</param>
    /// <param name="symbol">The symbol of the holding.</param>
    /// <returns>Returns 200 OK with the holding, or 404 if not found.</returns>
    [HttpGet("{symbol}")]
    [ProducesResponseType(typeof(HoldingDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int accountId, string symbol)
    {
        var holding = await _holdingService.GetHoldingAsync(accountId, new Symbol(symbol));
        if (holding is null) return NotFound();
        return Ok(HoldingMapper.ToDTO(holding));
    }

    /// <summary>
    /// Creates a new holding for the specified account.
    /// </summary>
    /// <param name="accountId">The account ID to add the holding to.</param>
    /// <param name="dto">The holding data including symbol and quantity.</param>
    /// <returns>Returns 200 OK with the created holding.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(HoldingDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(int accountId, [FromBody] HoldingDTO dto)
    {
        Symbol symbol = new Symbol(dto.Symbol);
        await _holdingService.AddHoldingAsync(accountId, symbol, dto.Quantity);
        var holding = await _holdingService.GetHoldingAsync(accountId, symbol);
        return Ok(HoldingMapper.ToDTO(holding!));
    }

    /// <summary>
    /// Updates the quantity of an existing holding.
    /// </summary>
    /// <param name="accountId">The account ID containing the holding.</param>
    /// <param name="symbol">The symbol of the holding to update.</param>
    /// <param name="dto">The new holding data including updated quantity.</param>
    /// <returns>
    /// Returns 200 OK with the updated holding, or 404 if the holding does not exist.
    /// </returns>
    [HttpPut("{symbol}")]
    [ProducesResponseType(typeof(HoldingDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int accountId, string symbol, [FromBody] HoldingDTO dto)
    {
        var holding = await _holdingService.GetHoldingAsync(accountId, new Symbol(symbol));
        if (holding is null) return NotFound();

        await _holdingService.UpdateHoldingQuantityAsync(holding, dto.Quantity);
        return Ok(HoldingMapper.ToDTO(holding));
    }

    /// <summary>
    /// Deletes a holding from the specified account.
    /// </summary>
    /// <param name="accountId">The account ID containing the holding.</param>
    /// <param name="symbol">The symbol of the holding to delete.</param>
    /// <returns>
    /// Returns 204 No Content if deleted successfully, or 404 if the holding does not exist.
    /// </returns>
    [HttpDelete("{symbol}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int accountId, string symbol)
    {
        var holding = await _holdingService.GetHoldingAsync(accountId, new Symbol(symbol));
        if (holding is null) return NotFound();

        await _holdingService.RemoveHoldingAsync(accountId, new Symbol(symbol));
        return NoContent();
    }
}
