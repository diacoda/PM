using PM.Domain.Entities;

namespace PM.Application.Interfaces;

public interface IHoldingRepository
{
    Task<Holding?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Holding>> ListByAccountAsync(int accountId, CancellationToken ct = default);
    Task AddAsync(Holding holding, CancellationToken ct = default);
    Task UpdateAsync(Holding holding, CancellationToken ct = default);
    Task DeleteAsync(Holding holding, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}