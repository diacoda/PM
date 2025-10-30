using PM.SharedKernel;

namespace PM.Application.Interfaces;
public interface IBaseRepository<TEntity>
    where TEntity : class
{
    Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TEntity?> GetByIdWithIncludesAsync(int id, IncludeOption[] includes, CancellationToken ct = default);
    Task<IEnumerable<TEntity>> ListAsync(CancellationToken ct = default);
    Task<IEnumerable<TEntity>> ListWithIncludesAsync(IncludeOption[] includes, CancellationToken ct = default);
}