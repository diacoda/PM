using Microsoft.AspNetCore.Mvc;
using PM.Application.Commands;
using PM.DTO.Prices;
using PM.Application.Interfaces;

namespace PM.API.Controllers
{
    /// <summary>
    /// Handles price data operations including fetching from providers, upserting, updating, retrieving, and deleting prices.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PricesController : ControllerBase
    {
        private readonly IPriceService _priceService;

        /// <summary>
        /// Initializes a new instance of <see cref="PricesController"/>.
        /// </summary>
        /// <param name="priceService">Service to manage price data persistence and retrieval.</param>
        public PricesController(
            IPriceService priceService)
        {
            _priceService = priceService ?? throw new ArgumentNullException(nameof(priceService));
        }

        /// <summary>
        /// Insert or update a price for a symbol and date.
        /// </summary>
        /// <param name="symbol">The ticker symbol.</param>
        /// <param name="request">Price update request payload.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Returns 200 OK with the updated price.</returns>
        [HttpPut("{symbol}")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(PriceDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PriceDTO>> UpdatePrice(
            string symbol,
            [FromBody] UpdatePriceRequest request,
            CancellationToken ct = default)
        {
            var dto = await _priceService.UpdatePriceAsync(symbol, request, ct);
            return Ok(dto);
        }

        /// <summary>
        /// Get a price for a specific symbol and date.
        /// </summary>
        /// <param name="symbol">Ticker symbol.</param>
        /// <param name="date">Date in YYYY-MM-DD format.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Returns 200 OK with the price, 404 if not found, or 400 for invalid date format.</returns>
        [HttpGet("{symbol}/{date}")]
        [ProducesResponseType(typeof(PriceDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PriceDTO>> GetPrice(string symbol, string date, CancellationToken ct = default)
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
        /// <param name="symbol">Ticker symbol.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Returns 200 OK with a list of prices, or 400 for invalid input.</returns>
        [HttpGet("{symbol}/history")]
        [ProducesResponseType(typeof(List<PriceDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<PriceDTO>>> GetAllPrices(string symbol, CancellationToken ct = default)
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
        /// <param name="symbol">Ticker symbol.</param>
        /// <param name="date">Date in YYYY-MM-DD format.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Returns 204 No Content if deleted, 404 if not found, or 400 for invalid input.</returns>
        [HttpDelete("{symbol}/{date}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletePrice(string symbol, string date, CancellationToken ct = default)
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
        /// <param name="date">Date in YYYY-MM-DD format.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Returns 200 OK with a list of prices, or 400 for invalid date format.</returns>
        [HttpGet("date/{date}")]
        [ProducesResponseType(typeof(List<PriceDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<PriceDTO>>> GetAllPricesByDate(string date, CancellationToken ct = default)
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
                return BadRequest(new ProblemDetails { Title = "Invalid date format. Use YYYY-MM-DD." });

            var prices = await _priceService.GetAllPricesByDateAsync(parsedDate, ct);
            return Ok(prices);
        }
    }
}
