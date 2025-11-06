using Microsoft.EntityFrameworkCore;
using PM.Application.Interfaces;
using PM.SharedKernel;

namespace PM.Infrastructure.Repositories;
public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity>
    where TEntity : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    protected BaseRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, ct);
    }

    public virtual async Task<TEntity?> GetByIdWithIncludesAsync(int id, IncludeOption[] includes, CancellationToken ct = default)
    {
        var query = ApplyIncludes(_dbSet.AsQueryable(), includes);
        return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, ct);
    }

    public virtual async Task<IEnumerable<TEntity>> ListAsync(CancellationToken ct = default)
    {
        return await _dbSet.ToListAsync(ct);
    }

    public virtual async Task<IEnumerable<TEntity>> ListWithIncludesAsync(IncludeOption[] includes, CancellationToken ct = default)
    {
        var query = ApplyIncludes(_dbSet.AsQueryable(), includes);
        return await query.ToListAsync(ct);
    }

    protected abstract IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query, IncludeOption[] includes);
}