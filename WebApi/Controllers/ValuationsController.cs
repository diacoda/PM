using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.Application.Services;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.DTO;

namespace PM.API.Controllers;

[ApiController]
//[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]

public class ValuationsController : ControllerBase
{
    private readonly IValuationService _valuationService;
    private readonly IPortfolioService _portfolioService;

    public ValuationsController(
        IValuationService valuationService,
        IPortfolioService portfolioService)
    {
        _valuationService = valuationService;
        _portfolioService = portfolioService;
    }

    [HttpPost("{portfolioId}")]
    public async Task<IActionResult> GeneratePortfolioValuations(int portfolioId, [FromBody] ValuationRequestDTO dto)
    {
        var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
        if (portfolio is null) return BadRequest(new ProblemDetails { Title = "Invalid portfolio" });
        await _valuationService.GenerateAndStorePortfolioValuations(portfolio, dto.Start, dto.End, new Currency(dto.Currency), ValuationPeriod.Daily);
        return Ok();
    }

    [HttpGet("{portfolioId}")]
    public async Task<IActionResult> GetValuations(int portfolioId)
    {
        var portfolio = await _portfolioService.GetByIdAsync(portfolioId);
        if (portfolio is null) return BadRequest(new ProblemDetails { Title = "Invalid portfolio" });
        var valuations = await _valuationService.GetByPortfolioAsync(portfolioId, ValuationPeriod.Daily);
        return Ok(valuations);
    }
}
