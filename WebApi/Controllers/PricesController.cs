namespace PM.API.Controllers;

//using InvestmentPortfolio.API.Filters;
using PM.Application.Commands;
using PM.DTO.Prices;
using PM.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class PricesController : ControllerBase
{
    private readonly FetchDailyPricesCommand _fetcher;
    private readonly IMarketCalendar _calendar;
    private readonly IPriceService _priceService;

    public PricesController(
        FetchDailyPricesCommand fetcher,
        IMarketCalendar calendar,
        IPriceService priceService)
    {
        _fetcher = fetcher ?? throw new ArgumentNullException(nameof(fetcher));
        _calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
        _priceService = priceService ?? throw new ArgumentNullException(nameof(priceService));
    }

    /// <summary>
    /// Trigger an on-demand price fetch for one or more tickers.
    /// </summary>
    /// <remarks>
    /// * Rejects future dates.  
    /// * For today's date, only allows fetching after market close unless <paramref name="allowMarketClosed"/> is set.  
    /// * Returns details of fetched prices.  
    /// </remarks>
    /// <param name="date">Optional date in YYYY-MM-DD format. Defaults to today.</param>
    /// <param name="allowMarketClosed">Allow fetching even if the market is closed (useful for historical/manual runs).</param>
    [HttpGet("fetch")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // replace with actual return type if known
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Fetch([FromQuery] string? date = null, [FromQuery] bool allowMarketClosed = false)
    {
        DateOnly fetchDate;
        if (!string.IsNullOrWhiteSpace(date))
        {
            if (!DateOnly.TryParse(date, out fetchDate))
            {
                return BadRequest(new ProblemDetails { Title = "Invalid date format. Use YYYY-MM-DD." });
            }
        }
        else
        {
            fetchDate = DateOnly.FromDateTime(DateTime.Today);
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        if (fetchDate > today)
            return BadRequest(new ProblemDetails { Title = "Cannot fetch prices for future dates." });

        if (fetchDate == today && !_calendar.IsAfterMarketClose("TSX"))
            return BadRequest(new ProblemDetails { Title = "Prices for today are only available after market close." });

        var result = await _fetcher.ExecuteAsync(fetchDate, allowMarketClosed);
        return Ok(result);
    }

    /// <summary>
    /// Insert or update a price for a symbol and date.
    /// </summary>
    [HttpPut("{symbol}")]
    [Consumes("application/json")]
    //[ServiceFilter(typeof(ValidateRequestFilter<UpdatePriceRequest>))]
    [ProducesResponseType(typeof(PriceDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PriceDTO>> UpdatePrice(
        string symbol,
        [FromBody] UpdatePriceRequest request,
        CancellationToken ct)
    {
        var dto = await _priceService.UpdatePriceAsync(symbol, request, ct);
        return Ok(dto);
    }

    /// <summary>
    /// Fetch a price from providers for a symbol and date, and upsert it.
    /// </summary>
    [HttpPost("fetch-provider")]
    [Consumes("application/json")]
    //[ServiceFilter(typeof(ValidateRequestFilter<UpsertPriceProviderRequest>))]
    [ProducesResponseType(typeof(PriceDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PriceDTO>> FetchPriceFromProvider(
        [FromBody] UpsertPriceProviderRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol))
            return BadRequest(new ProblemDetails { Title = "Symbol is required." });

        var dto = await _priceService.FetchAndUpsertFromProviderAsync(request, ct);
        return Ok(dto);
    }
    /// <summary>
    /// Get a price for a specific symbol and date.
    /// </summary>
    [HttpGet("{symbol}/{date}")]
    [ProducesResponseType(typeof(PriceDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PriceDTO>> GetPrice(string symbol, string date, CancellationToken ct)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest(new ProblemDetails { Title = "Invalid date format. Use YYYY-MM-DD." });

        try
        {
            var dto = await _priceService.GetPriceAsync(symbol, parsedDate, ct);
            if (dto is null)
                return NotFound(new ProblemDetails { Title = $"No price found for {symbol} on {date}." });

            return Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message });
        }
    }

    /// <summary>
    /// Get all historical prices for a symbol.
    /// </summary>
    [HttpGet("{symbol}/history")]
    [ProducesResponseType(typeof(List<PriceDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<PriceDTO>>> GetAllPrices(string symbol, CancellationToken ct)
    {
        try
        {
            var result = await _priceService.GetAllPricesForSymbolAsync(symbol, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message });
        }
    }

    /// <summary>
    /// Delete a price for a symbol and date.
    /// </summary>
    [HttpDelete("{symbol}/{date}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeletePrice(string symbol, string date, CancellationToken ct)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest(new ProblemDetails { Title = "Invalid date format. Use YYYY-MM-DD." });

        try
        {
            var deleted = await _priceService.DeletePriceAsync(symbol, parsedDate, ct);
            if (!deleted)
                return NotFound(new ProblemDetails { Title = $"Price not found for {symbol} on {date}." });

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message });
        }
    }
    /// <summary>
    /// Get all prices for all symbols on a given date.
    /// </summary>
    [HttpGet("date/{date}")]
    [ProducesResponseType(typeof(List<PriceDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<PriceDTO>>> GetAllPricesByDate(string date, CancellationToken ct)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest(new ProblemDetails { Title = "Invalid date format. Use YYYY-MM-DD." });

        var prices = await _priceService.GetAllPricesByDateAsync(parsedDate, ct);
        return Ok(prices);
    }
}
