namespace PM.DTO;

/// <summary>
/// Request object for generating portfolio valuations.
/// </summary>
/// <param name="Start">The start date of the valuation period.</param>
/// <param name="End">The end date of the valuation period.</param>
/// <param name="Currency">The currency code (e.g., "CAD", "USD") for the valuation.</param>
public record ValuationRequestDTO(DateTime Start, DateTime End, string Currency);
