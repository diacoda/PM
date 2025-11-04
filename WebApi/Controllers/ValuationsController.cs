using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Mappers;
using PM.Domain.Values;
using PM.DTO;

namespace PM.API.Controllers
{
    /// <summary>
    /// Provides endpoints for generating and retrieving portfolio valuations.
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
        [HttpPost("{portfolioId}")]
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

            await _valuationService.GenerateAndStorePortfolioValuations(
                portfolioId,
                dto.Date,
                new Currency(dto.Currency),
                ValuationPeriod.Daily,
                ct);

            return Ok();
        }

        /// <summary>
        /// Retrieves all daily valuations for a given portfolio.
        /// </summary>
        /// <param name="portfolioId">The ID of the portfolio.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// Returns a list of valuation records for the portfolio. Returns 400 Bad Request if the portfolio is invalid.
        /// </returns>
        [HttpGet("{portfolioId}")]
        [ProducesResponseType(typeof(IEnumerable<ValuationRecord>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetValuations(int portfolioId, CancellationToken ct = default)
        {
            var portfolio = await _portfolioService.GetByIdAsync(portfolioId, ct);
            if (portfolio is null)
                return BadRequest(new ProblemDetails { Title = "Invalid portfolio" });

            var valuations = await _valuationService.GetByPortfolioAsync(portfolioId, ValuationPeriod.Daily, ct);
            return Ok(valuations);
        }
    }
}
