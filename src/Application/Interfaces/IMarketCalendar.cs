namespace PM.Application.Interfaces;

public interface IMarketCalendar
{
    public bool IsMarketOpen(DateOnly date, string? market = "TSX");
    public bool IsAfterMarketClose(string market);
    public bool IsToday(DateOnly date);
}