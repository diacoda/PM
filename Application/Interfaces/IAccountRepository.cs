using PM.Domain.Entities;

namespace PM.Application.Interfaces;


public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Account>> ListByPortfolioAsync(int portfolioId, CancellationToken ct = default);
    Task AddAsync(Account account, CancellationToken ct = default);
    Task UpdateAsync(Account account, CancellationToken ct = default);
    Task DeleteAsync(Account account, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}