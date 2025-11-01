using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IPerformanceService
{
    Task<List<DailyReturn>> GetAccountDailyTwrAsync(
        Account account,
        DateTime start,
        DateTime end,
        Currency ccy,
        CancellationToken ct = default);
    Task<List<DailyReturn>> GetPortfolioDailyTwrAsync(
        Portfolio portfolio,
        DateTime start,
        DateTime end,
        Currency ccy,
        CancellationToken ct = default);
    decimal Link(IEnumerable<decimal> dailyReturns);
    Task<PeriodPerformance> GetAccountReturnAsync(
        Account account,
        DateTime start,
        DateTime end,
        Currency reportingCurrency,
        CancellationToken ct = default);
    Task<PeriodPerformance> GetPortfolioReturnAsync(
        Portfolio portfolio,
        DateTime start,
        DateTime end,
        Currency reportingCurrency,
        CancellationToken ct = default);
}
