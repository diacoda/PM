using System;

namespace PM.Domain.Entities;

/// <summary>
/// Compact risk summary computed from daily returns (and optionally vs benchmark).
/// All percentages are decimals (e.g., 0.108 == 10.8%).
/// MaxDrawdown is negative or zero; Peak/Trough are the dates bounding that drawdown.
/// </summary>
public record RiskCard(
    decimal VolAnnual,               // Annualized volatility (σ) from daily returns
    decimal MaxDrawdown,             // Peak-to-trough decline on wealth index (negative number)
    DateTime? PeakDate,              // Date of the peak preceding the max drawdown
    DateTime? TroughDate,            // Date of the trough of the max drawdown
    decimal Sharpe,                  // Approx Sharpe using Rf≈0 (period link / vol)
    decimal HitRateDaily,            // Fraction of up days, 0..1
    decimal? CorrelationToBenchmark  // Pearson correlation to benchmark daily returns (null if not supplied)
);
