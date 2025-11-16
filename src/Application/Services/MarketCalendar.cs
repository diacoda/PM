using PM.Application.Interfaces;
using PM.SharedKernel;

namespace PM.Application.Services;

/// <summary>
/// Provides market calendar utilities such as:
/// - Checking if a market is open on a given date
/// - Determining if today is after market close
/// - Calculating the next valid trading day
/// - Converting scheduled run times to actual next-run DateTime values
///
/// This service is used by price-fetching and valuation schedulers.
/// It abstracts holiday/weekend logic and market close times.
/// </summary>
public class MarketCalendar : IMarketCalendar
{
    /// <summary>
    /// A dictionary of holidays per exchange.
    /// Key: exchange ID (e.g., "TSX")
    /// Value: list of holiday dates for that exchange.
    ///
    /// Example:
    /// {
    ///     "TSX": [2025-01-01, 2025-12-25],
    ///     "NYSE": [2025-07-04, ...]
    /// }
    /// </summary>
    private readonly Dictionary<string, List<DateOnly>> _holidays;
    private readonly ISystemClock _clock;

    public MarketCalendar(
        Dictionary<string, List<DateOnly>> holidays,
        ISystemClock? clock = null)
    {
        _holidays = holidays;
        _clock = clock ?? new SystemClock();
    }

    /// <summary>
    /// Returns the market close time (as a DateTimeOffset) for the given date & exchange.
    /// Time is returned in U.S. Eastern Time, because TSX and NYSE both operate in EST/EDT.
    ///
    /// This is used by price fetchers to check if the market is closed *today*.
    /// </summary>
    public DateTimeOffset GetCloseTime(DateOnly date, string exchangeId)
    {
        // Market close time per exchange (currently all 4:00 PM)
        var closeTime = exchangeId switch
        {
            "TSX" => new TimeOnly(16, 0),
            "NYSE" => new TimeOnly(16, 0),
            _ => new TimeOnly(16, 0)
        };

        // Recombine DateOnly + TimeOnly into a DateTime
        var dateTime = date.ToDateTime(closeTime);

        // Convert to EST/EDT — this adjusts for daylight savings
        var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        return new DateTimeOffset(dateTime, easternZone.GetUtcOffset(dateTime));
    }

    /// <summary>
    /// Quick utility to check if the given DateOnly is today.
    /// Used to decide whether to check real-time "after market close" rules.
    /// </summary>
    public bool IsToday(DateOnly date) =>
        date == DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Checks whether the market is considered "open" on the given date.
    ///
    /// Rules:
    /// - Weekends: never open
    /// - If exchange has holidays configured: these dates are closed
    ///
    /// Does NOT check intraday hours (that's done inside IsAfterMarketClose).
    /// </summary>
    public bool IsMarketOpen(DateOnly date, string? market = "TSX")
    {
        // Market closed on weekends
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            return false;

        // Market closed if the day is listed as a holiday
        if (market != null && _holidays.TryGetValue(market, out var dates))
        {
            if (dates.Contains(date))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if the given date is a holiday for ANY exchange.
    /// Useful for UI or generic validation.
    /// </summary>
    public bool IsHoliday(DateOnly date) =>
        _holidays.Values.Any(list => list.Contains(date));

    /// <summary>
    /// Returns true if the *current system time* is past today's market close.
    /// This is used to prevent fetching prices too early on the same day.
    ///
    /// NOTE:
    /// This uses the local system clock. If the server is not in EST,
    /// "correctness" depends on the hosting environment.
    /// </summary>
    public bool IsAfterMarketClose(string market)
    {
        // Same 4 PM close time logic
        var close = market switch
        {
            "TSX" => new TimeOnly(16, 0),
            "NYSE" => new TimeOnly(16, 0),
            _ => new TimeOnly(16, 0)
        };

        // Compare current local time vs. market close time
        return TimeOnly.FromDateTime(_clock.Now) >= close;
    }

    /// <summary>
    /// Calculates the next trading day after `fromDate`.
    /// Skips weekends *and* configured exchange holidays.
    ///
    /// Used by:
    /// - Price job scheduling (next valid run date)
    /// - Valuation scheduling
    /// </summary>
    public DateOnly GetNextMarketDay(DateOnly fromDate, string market = "TSX")
    {
        var candidate = fromDate.AddDays(1);

        // Keep advancing until we find a day the market is open
        while (!IsMarketOpen(candidate, market))
            candidate = candidate.AddDays(1);

        return candidate;
    }

    /// <summary>
    /// Given a scheduled run time (like 17:00 = 5 PM), returns the next
    /// calendar DateTime when the background job should execute.
    ///
    /// Logic:
    /// - Determine next market day (skipping weekends/holidays)
    /// - Apply the configured run time to it
    ///
    /// This makes price jobs run only on valid trading days.
    /// </summary>
    public DateTime GetNextMarketRunDateTime(TimeSpan scheduledRunTime, string market = "TSX")
    {
        var today = DateOnly.FromDateTime(_clock.Now);
        var nextDay = GetNextMarketDay(today, market);
        var runTime = TimeOnly.FromTimeSpan(scheduledRunTime);

        return nextDay.ToDateTime(runTime);
    }

    /// <summary>
    /// Returns the next valuation date.
    /// If requireMarketOpen = true → skip weekends/holidays
    /// Otherwise → valuations can run every day (useful for cash-only portfolios)
    /// </summary>
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

    /// <summary>
    /// Returns the next run DateTime for the valuation process.
    ///
    /// Combines:
    /// - next valid valuation date
    /// - scheduled run time (e.g., nightly at 2 AM)
    ///
    /// Used by valuation hosted services.
    /// </summary>
    public DateTime GetNextValuationRunDateTime(TimeSpan runTime, bool requireMarketOpen)
    {
        var today = DateOnly.FromDateTime(_clock.Now);
        var date = GetNextValuationDate(today, requireMarketOpen);

        return date.ToDateTime(TimeOnly.FromTimeSpan(runTime));
    }
}
