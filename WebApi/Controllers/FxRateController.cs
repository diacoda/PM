using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.Domain.Values;

namespace PM.API.Controllers;

/// <summary>
/// Provides endpoints for managing and retrieving foreign exchange (FX) rates.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FxRatesController : ControllerBase
{
    private readonly IFxRateService _fxService;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="fxService"></param>
    public FxRatesController(IFxRateService fxService)
    {
        _fxService = fxService;
    }

    /// <summary>
    /// Retrieves the FX rate for a currency pair on a specific date.
    /// </summary>
    /// <param name="from">The source currency code (e.g., "USD").</param>
    /// <param name="to">The target currency code (e.g., "CAD").</param>
    /// <param name="date">The date for the FX rate in YYYY-MM-DD format.</param>
    /// <returns>The FX rate if found; otherwise, a 404 or 400 error.</returns>
    [HttpGet("{from}/{to}/{date}")]
    public async Task<IActionResult> GetRate(string from, string to, string date)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
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
    /// Inserts or updates an FX rate for a currency pair on a specific date.
    /// </summary>
    /// <param name="from">The source currency code.</param>
    /// <param name="to">The target currency code.</param>
    /// <param name="rate">The FX rate value.</param>
    /// <param name="date">Optional date for the rate (defaults to today if not provided).</param>
    /// <returns>The updated FX rate object.</returns>
    [HttpPut("{from}/{to}")]
    public async Task<IActionResult> UpdateRate(
        string from,
        string to,
        [FromBody] decimal rate,
        [FromQuery] string? date = null)
    {
        DateOnly fxDate = date is null ? DateOnly.FromDateTime(DateTime.Today) : DateOnly.Parse(date);

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
    /// Retrieves all historical FX rates for a currency pair.
    /// </summary>
    /// <param name="from">The source currency code.</param>
    /// <param name="to">The target currency code.</param>
    /// <returns>A list of FX rates for the currency pair.</returns>
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
    /// Deletes an FX rate for a currency pair on a specific date.
    /// </summary>
    /// <param name="from">The source currency code.</param>
    /// <param name="to">The target currency code.</param>
    /// <param name="date">The date of the FX rate to delete (YYYY-MM-DD).</param>
    /// <returns>No content if successful; otherwise, a 404 or 400 error.</returns>
    [HttpDelete("{from}/{to}/{date}")]
    public async Task<IActionResult> DeleteRate(string from, string to, string date)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
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
    /// Retrieves all FX rates for a specific date.
    /// </summary>
    /// <param name="date">The date for which to retrieve FX rates (YYYY-MM-DD).</param>
    /// <returns>A list of FX rates for the specified date.</returns>
    [HttpGet("date/{date}")]
    public async Task<IActionResult> GetAllRatesByDate(string date)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest(new ProblemDetails { Title = "Invalid date format. Use YYYY-MM-DD." });

        var rates = await _fxService.GetAllRatesByDateAsync(parsedDate);
        return Ok(rates);
    }
}