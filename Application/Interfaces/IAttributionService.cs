using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IAttributionService
{
    Task<IEnumerable<ContributionRecord>> ContributionBySecurityAsync(Account account, DateTime start, DateTime end, Currency ccy);
    Task<IEnumerable<ContributionRecord>> ContributionBySecurityAsync(Portfolio portfolio, DateTime start, DateTime end, Currency ccy);
    Task<IEnumerable<ContributionRecord>> ContributionByAssetClassAsync(Portfolio portfolio, DateTime start, DateTime end, Currency ccy);
}
