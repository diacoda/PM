using System;

namespace PM.Domain.Entities;

/// <summary>
/// Linked performance over standard rolling windows as of a specific date.
/// All values are decimals where 0.0123 == 1.23%.
/// </summary>
public record RollingReturnSet(
    DateTime AsOf,
    decimal R_1M,
    decimal R_3M,
    decimal R_6M,
    decimal R_YTD,
    decimal R_1Y,
    decimal R_3Y,
    decimal R_SI
);