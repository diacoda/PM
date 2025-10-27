using PM.Domain.Entities;

namespace PM.Application.Interfaces;

public interface IAnalyticsService
{
    decimal Link(IEnumerable<decimal> dailyReturns);
    RollingReturnSet ComputeRolling(DailyReturn[] series, DateTime asOf, DateTime seriesStart);
    RiskCard ComputeRisk(DailyReturn[] series, DailyReturn[]? benchmark = null);
    Dictionary<(int Year, int Month), decimal> CalendarMonthlyReturns(DailyReturn[] series);
}