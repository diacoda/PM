using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.DTO;

namespace PM.API.Controllers
{
    /// <summary>
    /// Handles CRUD operations for tags used in portfolios, holdings, or other entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CashFlowsController : ControllerBase
    {
        private readonly ICashFlowService _cashFlowService;

        /// <summary>
        /// Initializes a new instance of <see cref="CashFlowsController"/>.
        /// </summary>
        /// <param name="cashFlowService">Service for managing cash flows.</param>
        public CashFlowsController(ICashFlowService cashFlowService)
        {
            _cashFlowService = cashFlowService;
        }

        /// <summary> /// <summary>
        /// Deletes a cash flow by ID.
        /// </summary>
        /// <param name="id">The ID of the cash flow to delete.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Returns 204 No Content if deleted, or 404 if the cash flow does not exist.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            try
            {
                await _cashFlowService.DeleteCashFlowAsync(id, ct);
            }
            catch (Exception ex)
            {
                return NotFound(new ProblemDetails { Title = "Cash flow not found.", Detail = ex.Message });
            }
            return NoContent();
        }
    }
}
