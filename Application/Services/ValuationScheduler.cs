using PM.Application.Interfaces;
using PM.Domain.Enums;

namespace PM.Application.Services;

public class ValuationScheduler
{
    private readonly IMarketCalendar _marketCalendar;

    public ValuationScheduler(IMarketCalendar marketCalendar)
    {
        _marketCalendar = marketCalendar;
    }

    /// <summary>
    /// Determines which valuation periods should run today.
    /// Always includes Daily, adds Weekly/Monthly/Quarterly/Yearly if today is a checkpoint.
    /// </summary>
    public IEnumerable<ValuationPeriod> GetValuationsForToday(DateTime date)
    {
        var periods = new List<ValuationPeriod> { ValuationPeriod.Daily };
        var dateOnly = DateOnly.FromDateTime(date);

        // Weekly: run if today is last market day of the week
        if (IsEndOfWeek(dateOnly))
            periods.Add(ValuationPeriod.Weekly);

        // Monthly: run if today is last market day of the month
        if (IsEndOfMonth(dateOnly))
            periods.Add(ValuationPeriod.Monthly);

        // Quarterly: run if today is last market day of the quarter
        if (IsEndOfQuarter(dateOnly))
            periods.Add(ValuationPeriod.Quarterly);

        // Yearly: run if today is last market day of the year
        if (IsEndOfYear(dateOnly))
            periods.Add(ValuationPeriod.Yearly);

        return periods;
    }

    private bool IsEndOfWeek(DateOnly date)
    {
        // Find last market day of the week
        var lastDay = date;
        while (!_marketCalendar.IsMarketOpen(lastDay) && lastDay.DayOfWeek != DayOfWeek.Monday)
        {
            lastDay = lastDay.AddDays(-1);
        }

        return date == lastDay && date.DayOfWeek == DayOfWeek.Friday;
    }

    private bool IsEndOfMonth(DateOnly date)
    {
        var lastDayOfMonth = new DateOnly(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        // roll back if it's a holiday or weekend
        while (!_marketCalendar.IsMarketOpen(lastDayOfMonth))
        {
            lastDayOfMonth = lastDayOfMonth.AddDays(-1);
        }
        return date == lastDayOfMonth;
    }

    private bool IsEndOfQuarter(DateOnly date)
    {
        int quarterEndMonth = (date.Month - 1) / 3 * 3 + 3; // 3,6,9,12
        var lastDayOfQuarter = new DateOnly(date.Year, quarterEndMonth, DateTime.DaysInMonth(date.Year, quarterEndMonth));
        while (!_marketCalendar.IsMarketOpen(lastDayOfQuarter))
        {
            lastDayOfQuarter = lastDayOfQuarter.AddDays(-1);
        }
        return date == lastDayOfQuarter;
    }

    private bool IsEndOfYear(DateOnly date)
    {
        var lastDayOfYear = new DateOnly(date.Year, 12, 31);
        while (!_marketCalendar.IsMarketOpen(lastDayOfYear))
        {
            lastDayOfYear = lastDayOfYear.AddDays(-1);
        }
        return date == lastDayOfYear;
    }
}
