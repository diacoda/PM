using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.Domain.Values;

namespace PM.API.Controllers
{
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
        /// Initializes a new instance of the <see cref="FxRatesController"/> class.
        /// </summary>
        /// <param name="fxService">Service used to manage and retrieve FX rates.</param>
        public FxRatesController(IFxRateService fxService)
        {
            _fxService = fxService;
        }

        /// <summary>
        /// Retrieves the FX rate for a specific currency pair on a given date.
        /// </summary>
        /// <param name="from">The source currency code (e.g., "USD").</param>
        /// <param name="to">The target currency code (e.g., "CAD").</param>
        /// <param name="date">The date for which the FX rate is requested in YYYY-MM-DD format.</param>
        /// <param name="ct">Cancellation token for request.</param>
        /// <returns>An <see cref="IActionResult"/> containing the FX rate if found; otherwise, a 404 or 400 error.</returns>
        [HttpGet("{from}/{to}/{date}")]
        public async Task<IActionResult> GetRate(
            string from,
            string to,
            string date,
            CancellationToken ct = default)
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
                return BadRequest(new ProblemDetails { Title = "Invalid date format. Use YYYY-MM-DD." });

            try
            {
                var rate = await _fxService.GetRateAsync(from.ToUpperInvariant(), to.ToUpperInvariant(), parsedDate, ct);
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
        /// Inserts or updates an FX rate for a currency pair.
        /// </summary>
        /// <param name="from">The source currency code (e.g., "USD").</param>
        /// <param name="to">The target currency code (e.g., "CAD").</param>
        /// <param name="rate">The FX rate value.</param>
        /// <param name="date">Optional date for the FX rate (defaults to today if not provided).</param>
        /// <param name="ct">Cancellation token for request.</param>
        /// <returns>An <see cref="IActionResult"/> containing the updated FX rate.</returns>
        [HttpPut("{from}/{to}")]
        public async Task<IActionResult> UpdateRate(
            string from,
            string to,
            [FromBody] decimal rate,
            [FromQuery] string? date = null,
            CancellationToken ct = default)
        {
            DateOnly fxDate = date is null ? DateOnly.FromDateTime(DateTime.Today) : DateOnly.Parse(date);

            try
            {
                var updated = await _fxService.UpdateRateAsync(from.ToUpperInvariant(), to.ToUpperInvariant(), rate, fxDate, ct);
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ProblemDetails { Title = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves the historical FX rates for a specific currency pair.
        /// </summary>
        /// <param name="from">The source currency code.</param>
        /// <param name="to">The target currency code.</param>
        /// <param name="ct">Cancellation token for request.</param>
        /// <returns>An <see cref="IActionResult"/> containing a list of FX rates for the currency pair.</returns>
        [HttpGet("{from}/{to}/history")]
        public async Task<IActionResult> GetAllRatesForPair(
            string from,
            string to,
            CancellationToken ct = default)
        {
            try
            {
                var rates = await _fxService.GetAllRatesForPairAsync(from.ToUpperInvariant(), to.ToUpperInvariant(), ct);
                return Ok(rates);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ProblemDetails { Title = ex.Message });
            }
        }

        /// <summary>
        /// Deletes an FX rate for a specific currency pair on a given date.
        /// </summary>
        /// <param name="from">The source currency code.</param>
        /// <param name="to">The target currency code.</param>
        /// <param name="date">The date of the FX rate to delete in YYYY-MM-DD format.</param>
        /// <param name="ct">Cancellation token for request.</param>
        /// <returns>No content if deletion is successful; otherwise, 404 or 400 error.</returns>
        [HttpDelete("{from}/{to}/{date}")]
        public async Task<IActionResult> DeleteRate(
            string from,
            string to,
            string date,
            CancellationToken ct = default)
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
                return BadRequest(new ProblemDetails { Title = "Invalid date format. Use YYYY-MM-DD." });

            try
            {
                var deleted = await _fxService.DeleteRateAsync(from.ToUpperInvariant(), to.ToUpperInvariant(), parsedDate, ct);
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
        /// <param name="date">The date to retrieve FX rates for in YYYY-MM-DD format.</param>
        /// <param name="ct">Cancellation token for request.</param>
        /// <returns>An <see cref="IActionResult"/> containing a list of FX rates for the specified date.</returns>
        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetAllRatesByDate(
            string date,
            CancellationToken ct)
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
                return BadRequest(new ProblemDetails { Title = "Invalid date format. Use YYYY-MM-DD." });

            var rates = await _fxService.GetAllRatesByDateAsync(parsedDate, ct);
            return Ok(rates);
        }
    }
}
