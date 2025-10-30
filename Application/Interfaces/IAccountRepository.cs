using PM.Domain.Entities;
using PM.SharedKernel;

namespace PM.Application.Interfaces;


public interface IAccountRepository : IBaseRepository<Account>
{
    Task<IEnumerable<Account>> ListByPortfolioAsync(int portfolioId, CancellationToken ct = default);
    Task<IEnumerable<Account>> ListByPortfolioWithIncludesAsync(int portfolioId, IncludeOption[] includes, CancellationToken ct = default);
    Task AddAsync(Account account, CancellationToken ct = default);
    Task UpdateAsync(Account account, CancellationToken ct = default);
    Task DeleteAsync(Account account, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}