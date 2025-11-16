using PM.Application.Interfaces;

namespace PM.Application.Services;

public class MarketCalendar : IMarketCalendar
{
    private readonly Dictionary<string, List<DateOnly>> _holidays;

    public MarketCalendar(Dictionary<string, List<DateOnly>> holidays)
    {
        _holidays = holidays;
    }

    public DateTimeOffset GetCloseTime(DateOnly date, string exchangeId)
    {
        var closeTime = exchangeId switch
        {
            "TSX" => new TimeOnly(16, 0),
            "NYSE" => new TimeOnly(16, 0),
            _ => new TimeOnly(16, 0)
        };

        var dateTime = date.ToDateTime(closeTime);
        var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        return new DateTimeOffset(dateTime, easternZone.GetUtcOffset(dateTime));
    }

    public bool IsToday(DateOnly date) =>
        date == DateOnly.FromDateTime(DateTime.Today);

    public bool IsMarketOpen(DateOnly date, string? market = "TSX")
    {
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            return false;

        if (market != null && _holidays.TryGetValue(market, out var dates))
        {
            if (dates.Contains(date))
                return false;
        }

        return true;
    }

    public bool IsHoliday(DateOnly date) =>
        _holidays.Values.Any(list => list.Contains(date));

    public bool IsAfterMarketClose(string market)
    {
        var close = market switch
        {
            "TSX" => new TimeOnly(16, 0),
            "NYSE" => new TimeOnly(16, 0),
            _ => new TimeOnly(16, 0)
        };

        return TimeOnly.FromDateTime(DateTime.Now) >= close;
    }

    // NEW: calculate next open market day
    public DateOnly GetNextMarketDay(DateOnly fromDate, string market = "TSX")
    {
        var candidate = fromDate.AddDays(1);

        while (!IsMarketOpen(candidate, market))
            candidate = candidate.AddDays(1);

        return candidate;
    }

    // NEW: Calculate next run DateTime for the price job
    public DateTime GetNextMarketRunDateTime(TimeSpan scheduledRunTime, string market = "TSX")
    {
        var nextDay = GetNextMarketDay(DateOnly.FromDateTime(DateTime.Today), market);
        var runTime = TimeOnly.FromTimeSpan(scheduledRunTime);

        return nextDay.ToDateTime(runTime);
    }

    public DateOnly GetNextValuationDate(DateOnly today, bool requireMarketOpen)
    {
        var candidate = today;

        while (true)
        {
            if (!requireMarketOpen)
                return candidate;

            if (IsMarketOpen(candidate))
                return candidate;

            candidate = candidate.AddDays(1);
        }
    }

    public DateTime GetNextValuationRunDateTime(TimeSpan runTime, bool requireMarketOpen)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var date = GetNextValuationDate(today, requireMarketOpen);

        return date.ToDateTime(TimeOnly.FromTimeSpan(runTime));
    }

}
