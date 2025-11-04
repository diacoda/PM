using PM.Application.Interfaces;

namespace PM.Application.Services;

public class MarketCalendar : IMarketCalendar
{
    private readonly Dictionary<string, List<DateOnly>> _holidays;

    public MarketCalendar(Dictionary<string, List<DateOnly>> holidays)
    {
        _holidays = holidays;
    }

    public bool IsToday(DateOnly date)
    {
        return date == DateOnly.FromDateTime(DateTime.Today);
    }
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

    public bool IsHoliday(DateOnly date)
    {
        return _holidays.Values.Any(list => list.Contains(date));
    }
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
