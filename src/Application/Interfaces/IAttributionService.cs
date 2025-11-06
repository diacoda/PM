using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IAttributionService
{
    Task<IEnumerable<ContributionRecord>> ContributionBySecurityAsync(Account account, DateOnly start, DateOnly end, Currency ccy, CancellationToken ct = default);
    Task<IEnumerable<ContributionRecord>> ContributionBySecurityAsync(Portfolio portfolio, DateOnly start, DateOnly end, Currency ccy, CancellationToken ct = default);
    Task<IEnumerable<ContributionRecord>> ContributionByAssetClassAsync(Portfolio portfolio, DateOnly start, DateOnly end, Currency ccy, CancellationToken ct = default);
}
