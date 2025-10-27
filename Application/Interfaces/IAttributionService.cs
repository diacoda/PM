using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IAttributionService
{
    IEnumerable<ContributionRecord> ContributionBySecurity(Account account, DateTime start, DateTime end, Currency ccy);
    IEnumerable<ContributionRecord> ContributionBySecurity(Portfolio portfolio, DateTime start, DateTime end, Currency ccy);
    IEnumerable<ContributionRecord> ContributionByAssetClass(Portfolio portfolio, DateTime start, DateTime end, Currency ccy);
}
