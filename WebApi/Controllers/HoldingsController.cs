using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.DTO;
using PM.Domain.Values;
using PM.Domain.Mappers;

namespace PM.API.Controllers;

[ApiController]
[Route("api/portfolios/{portfolioId}/accounts/{accountId}/holdings")]
[Produces("application/json")]
public class HoldingsController : ControllerBase
{
    private readonly IHoldingService _holdingService;

    public HoldingsController(IHoldingService holdingService)
    {
        _holdingService = holdingService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<HoldingDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(int accountId)
    {
        var holdings = await _holdingService.ListHoldingsAsync(accountId);
        return Ok(holdings.Select(HoldingMapper.ToDTO));
    }

    [HttpGet("{symbol}")]
    [ProducesResponseType(typeof(HoldingDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int accountId, string symbol)
    {
        var holding = await _holdingService.GetHoldingAsync(accountId, new Symbol(symbol));
        if (holding is null) return NotFound();
        return Ok(HoldingMapper.ToDTO(holding));
    }

    [HttpPost]
    [ProducesResponseType(typeof(HoldingDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(int accountId, [FromBody] HoldingDTO dto)
    {
        Symbol symbol = new Symbol(dto.Symbol);
        await _holdingService.AddHoldingAsync(accountId, symbol, dto.Quantity);
        var holding = await _holdingService.GetHoldingAsync(accountId, symbol);
        return Ok(HoldingMapper.ToDTO(holding!));
    }

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