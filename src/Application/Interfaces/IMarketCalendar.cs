namespace PM.Application.Interfaces;

public interface IMarketCalendar
{
    public bool IsMarketOpen(DateOnly date, string? market = "TSX");
    public bool IsAfterMarketClose(string market);
    public bool IsToday(DateOnly date);
    DateTimeOffset GetCloseTime(DateOnly date, string exchangeId);
    DateOnly GetNextMarketDay(DateOnly fromDate, string market = "TSX");
    DateTime GetNextMarketRunDateTime(TimeSpan scheduledRunTime, string market = "TSX");
    DateOnly GetNextValuationDate(DateOnly today, bool requireMarketOpen);
    DateTime GetNextValuationRunDateTime(TimeSpan runTime, bool requireMarketOpen);
}