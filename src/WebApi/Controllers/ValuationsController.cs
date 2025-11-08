using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.DTO;
using PM.Domain.Mappers;

namespace PM.API.Controllers
{
    /// <summary>
    /// Provides endpoints for retrieving and calculating valuations for portfolios and accounts.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ValuationsController : ControllerBase
    {
        private readonly IValuationService _valuationService;
        private readonly IPortfolioService _portfolioService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValuationsController"/> class.
        /// </summary>
        /// <param name="valuationService">Service for calculating and storing valuations.</param>
        /// <param name="portfolioService">Service for accessing portfolios.</param>
        public ValuationsController(
            IValuationService valuationService,
            IPortfolioService portfolioService)
        {
            _valuationService = valuationService;
            _portfolioService = portfolioService;
        }

        /// <summary>
        /// Generates and stores daily valuations for a portfolio within a specified date range.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio to value.</param>
        /// <param name="dto">The valuation request containing start date, end date, and currency.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// Returns 200 OK if valuations were successfully generated, or 400 Bad Request if the portfolio is invalid.
        /// </returns>
        [HttpGet("{portfolioId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GeneratePortfolioValuations(
            int portfolioId,
            [FromBody] ValuationRequestDTO dto,
            CancellationToken ct = default)
        {
            var portfolio = await _portfolioService.GetByIdAsync(portfolioId, ct);
            if (portfolio is null)
                return BadRequest(new ProblemDetails { Title = "Invalid portfolio" });

            await _valuationService.GeneratePortfolioValuationSnapshot(
                portfolioId,
                dto.Date,
                new Currency(dto.Currency),
                ct);

            return Ok();
        }

        // ---------------- GET LATEST ----------------

        /// <summary>
        /// Gets the latest valuation for a given portfolio or account.
        /// </summary>
        [HttpGet("latest")]
        [ProducesResponseType(typeof(ValuationSnapshotDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLatestAsync(
            [FromQuery] EntityKind kind,
            [FromQuery] int entityId,
            [FromQuery] string currency,
            [FromQuery] ValuationPeriod? period,
            CancellationToken ct = default)
        {
            var result = await _valuationService.GetLatestAsync(
                kind,
                entityId,
                new Currency(currency),
                period,
                includeAssetClass: false,
                ct);

            if (result is null)
                return NotFound();

            return Ok(result.ToDTO());
        }

        // ---------------- GET RANGE ----------------

        /// <summary>
        /// Gets all valuation records for an entity within a date range.
        /// </summary>
        /// <param name="kind">Entity kind: "Portfolio" or "Account".</param>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="start">Start date (YYYY-MM-DD).</param>
        /// <param name="end">End date (YYYY-MM-DD).</param>
        /// <param name="currency">Currency code (e.g. "CAD").</param>
        /// <param name="period">Optional valuation period (e.g. Daily).</param>
        /// <param name="ct">Cancellation token</param>
        [HttpGet("range")]
        [ProducesResponseType(typeof(IEnumerable<ValuationSnapshotDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRangeAsync(
            [FromQuery] EntityKind kind,
            [FromQuery] int entityId,
            [FromQuery] DateOnly start,
            [FromQuery] DateOnly end,
            [FromQuery] string currency,
            [FromQuery] ValuationPeriod? period,
            CancellationToken ct = default)
        {
            var records = await _valuationService.GetHistoryAsync(kind, entityId, start, end, new Currency(currency), period, ct);
            return Ok(records.Select(r => r.ToDTO()));
        }

        // ---------------- GET AS OF DATE ----------------

        /// <summary>
        /// Gets all valuations as of a specific date for the given entity kind.
        /// </summary>
        [HttpGet("asof")]
        [ProducesResponseType(typeof(IEnumerable<ValuationSnapshotDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAsOfDateAsync(
            [FromQuery] EntityKind kind,
            [FromQuery] DateOnly date,
            [FromQuery] string currency,
            [FromQuery] ValuationPeriod? period,
            CancellationToken ct = default)
        {
            var records = await _valuationService.GetAsOfDateAsync(kind, date, new Currency(currency), period, ct);
            return Ok(records.Select(r => r.ToDTO()));
        }

    }
}
