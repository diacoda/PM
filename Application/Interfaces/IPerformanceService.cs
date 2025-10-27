using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IPerformanceService
{
    IAsyncEnumerable<DailyReturn> GetAccountDailyTwrAsync(Account account, DateTime start, DateTime end, Currency ccy);
    IAsyncEnumerable<DailyReturn> GetPortfolioDailyTwrAsync(Portfolio portfolio, DateTime start, DateTime end, Currency ccy);
    decimal Link(IEnumerable<decimal> dailyReturns);
    Task<PeriodPerformance> GetAccountReturnAsync(Account account, DateTime start, DateTime end, Currency reportingCurrency);
    Task<PeriodPerformance> GetPortfolioReturnAsync(Portfolio portfolio, DateTime start, DateTime end, Currency reportingCurrency);
}
