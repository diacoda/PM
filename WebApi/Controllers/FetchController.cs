using Microsoft.AspNetCore.Mvc;
using PM.Application.Commands;
using PM.DTO.Prices;
using PM.Application.Interfaces;

namespace PM.API.Controllers;

/// <summary>
/// Handles price data operations including fetching from providers, upserting, updating, retrieving, and deleting prices.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FetchController : ControllerBase
{
    private readonly FetchDailyPricesCommand _fetcher;
    private readonly IMarketCalendar _calendar;


    /// <summary>
    /// Initializes a new instance of <see cref="PricesController"/>.
    /// </summary>
    /// <param name="fetcher">Command to fetch daily prices from providers.</param>
    /// <param name="calendar">Market calendar service to validate market hours and holidays.</param>
    public FetchController(
        FetchDailyPricesCommand fetcher,
        IMarketCalendar calendar
        )
    {
        _fetcher = fetcher ?? throw new ArgumentNullException(nameof(fetcher));
        _calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
    }

    /// <summary>
    /// Trigger an on-demand price fetch for one or more tickers.
    /// </summary>
    /// <remarks>
    /// - Rejects future dates.  
    /// - For today's date, only allows fetching after market close unless <paramref name="allowMarketClosed"/> is set.  
    /// - Returns details of fetched prices.  
    /// </remarks>
    /// <param name="date">Optional date in YYYY-MM-DD format. Defaults to today.</param>
    /// <param name="allowMarketClosed">Allow fetching even if the market is closed (useful for historical/manual runs).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Returns 200 OK with fetched prices or 400 Bad Request for invalid input.</returns>
    [HttpGet()]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // replace with actual return type
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Fetch([FromQuery] string? date = null, [FromQuery] bool allowMarketClosed = false, CancellationToken ct = default)
    {
        DateOnly fetchDate;
        if (!string.IsNullOrWhiteSpace(date))
        {
            if (!DateOnly.TryParse(date, out fetchDate))
                return BadRequest(new ProblemDetails { Title = "Invalid date format. Use YYYY-MM-DD." });
        }
        else
        {
            fetchDate = DateOnly.FromDateTime(DateTime.Today);
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        if (fetchDate > today)
            return BadRequest(new ProblemDetails { Title = "Cannot fetch prices for future dates." });

        if (fetchDate == today && !_calendar.IsAfterMarketClose("TSX") && !allowMarketClosed)
            return BadRequest(new ProblemDetails { Title = "Prices for today are only available after market close." });

        var result = await _fetcher.ExecuteAsync(fetchDate, allowMarketClosed, ct);
        return Ok(result);
    }
}
