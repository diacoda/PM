using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.Domain.Values;

namespace PM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class FxRatesController : ControllerBase
{
    private readonly IFxRateService _fxService;

    public FxRatesController(IFxRateService fxService)
    {
        _fxService = fxService;
    }

    /// <summary>
    /// Get FX rate for a currency pair on a specific date
    /// </summary>
    [HttpGet("{from}/{to}/{date}")]
    public async Task<IActionResult> GetRate(string from, string to, string date)
    {
        if (!DateTime.TryParse(date, out var parsedDate))
            return BadRequest(new ProblemDetails { Title = "Invalid date format. Use YYYY-MM-DD." });

        try
        {
            var rate = await _fxService.GetRateAsync(from, to, parsedDate);
            if (rate is null)
                return NotFound(new ProblemDetails { Title = $"No FX rate found for {from}/{to} on {date}" });

            return Ok(rate);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message });
        }
    }

    /// <summary>
    /// Insert or update an FX rate
    /// </summary>
    [HttpPut("{from}/{to}")]
    public async Task<IActionResult> UpdateRate(
        string from,
        string to,
        [FromBody] decimal rate,
        [FromQuery] string? date = null)
    {
        DateTime fxDate = date is null ? DateTime.Today : DateTime.Parse(date);

        try
        {
            var updated = await _fxService.UpdateRateAsync(from, to, rate, fxDate);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message });
        }
    }

    /// <summary>
    /// Get all historical FX rates for a currency pair
    /// </summary>
    [HttpGet("{from}/{to}/history")]
    public async Task<IActionResult> GetAllRatesForPair(string from, string to)
    {
        try
        {
            var rates = await _fxService.GetAllRatesForPairAsync(from, to);
            return Ok(rates);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message });
        }
    }

    /// <summary>
    /// Delete an FX rate for a currency pair and date
    /// </summary>
    [HttpDelete("{from}/{to}/{date}")]
    public async Task<IActionResult> DeleteRate(string from, string to, string date)
    {
        if (!DateTime.TryParse(date, out var parsedDate))
            return BadRequest(new ProblemDetails { Title = "Invalid date format. Use YYYY-MM-DD." });

        try
        {
            var deleted = await _fxService.DeleteRateAsync(from, to, parsedDate);
            if (!deleted)
                return NotFound(new ProblemDetails { Title = $"FX rate not found for {from}/{to} on {date}" });

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message });
        }
    }

    /// <summary>
    /// Get all FX rates for a given date
    /// </summary>
    [HttpGet("date/{date}")]
    public async Task<IActionResult> GetAllRatesByDate(string date)
    {
        if (!DateTime.TryParse(date, out var parsedDate))
            return BadRequest(new ProblemDetails { Title = "Invalid date format. Use YYYY-MM-DD." });

        var rates = await _fxService.GetAllRatesByDateAsync(parsedDate);
        return Ok(rates);
    }
}
