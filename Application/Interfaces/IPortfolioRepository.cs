using PM.Domain.Entities;

namespace PM.Application.Interfaces;

public interface IPortfolioRepository
{
    Task<Portfolio?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Portfolio>> ListAsync(CancellationToken ct = default);
    Task AddAsync(Portfolio portfolio, CancellationToken ct = default);
    Task UpdateAsync(Portfolio portfolio, CancellationToken ct = default);
    Task DeleteAsync(Portfolio portfolio, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}