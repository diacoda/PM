using PM.Domain.Entities;

namespace PM.Application.Interfaces;

public interface IPortfolioRepository : IBaseRepository<Portfolio>
{
    Task AddAsync(Portfolio portfolio, CancellationToken ct = default);
    Task UpdateAsync(Portfolio portfolio, CancellationToken ct = default);
    Task DeleteAsync(Portfolio portfolio, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}