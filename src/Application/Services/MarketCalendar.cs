using PM.Application.Interfaces;

namespace PM.Application.Services;

public class MarketCalendar : IMarketCalendar
{
    private readonly Dictionary<string, List<DateOnly>> _holidays;

    public MarketCalendar(Dictionary<string, List<DateOnly>> holidays)
    {
        _holidays = holidays;
    }
    /// <summary>
    /// Gets the market close time for a given date and exchange.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="exchangeId"></param>
    /// <returns></returns>
    public DateTimeOffset GetCloseTime(DateOnly date, string exchangeId)
    {
        // Define market close times (local time)
        var closeTime = exchangeId switch
        {
            "TSX" => new TimeOnly(16, 0), // Toronto Stock Exchange closes at 4 PM ET
            "NYSE" => new TimeOnly(16, 0), // NYSE closes at 4 PM ET
            _ => new TimeOnly(16, 0) // Default close time
        };

        // Convert DateOnly + TimeOnly to DateTimeOffset (assuming Eastern Time)
        var dateTime = date.ToDateTime(closeTime);
        var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        return new DateTimeOffset(dateTime, easternZone.GetUtcOffset(dateTime));
    }
    /// <summary>
    /// Checks if the given date is today.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public bool IsToday(DateOnly date)
    {
        return date == DateOnly.FromDateTime(DateTime.Today);
    }
    /// <summary>
    /// Checks if the market is open on the given date.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="market"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Checks if the given date is a holiday.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public bool IsHoliday(DateOnly date)
    {
        return _holidays.Values.Any(list => list.Contains(date));
    }

    /// <summary>
    /// Checks if the current time is after market close for the given market.
    /// </summary>
    /// <param name="market"></param>
    /// <returns></returns>
    public bool IsAfterMarketClose(string market)
    {
        var close = market switch
        {
            "TSX" => new TimeOnly(16, 0),
            "NYSE" => new TimeOnly(16, 0),
            _ => new TimeOnly(16, 0)
        };

        var now = TimeOnly.FromDateTime(DateTime.Now);
        return now >= close;
    }
}
