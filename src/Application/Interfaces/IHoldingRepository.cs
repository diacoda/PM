using PM.Domain.Entities;

namespace PM.Application.Interfaces;

public interface IHoldingRepository
{
    Task<Holding?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Holding>> ListByAccountAsync(int accountId, CancellationToken ct = default);
}