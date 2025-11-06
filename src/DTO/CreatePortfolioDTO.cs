using System;

namespace PM.DTO;

/// <summary>
/// Data Transfer Object used to create a new investment portfolio.
/// </summary>
/// <param name="Owner">The name of the portfolio owner.</param>
public record CreatePortfolioDTO(string Owner);
