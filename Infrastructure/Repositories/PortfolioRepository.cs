using Microsoft.EntityFrameworkCore;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Infrastructure.Data;
using PM.SharedKernel;

namespace PM.Infrastructure.Repositories
{
    public class PortfolioRepository : BaseRepository<Portfolio>, IPortfolioRepository
    {
        public PortfolioRepository(AppDbContext db) : base(db)
        {
        }

        /// <summary>
        /// Applies conditional includes based on the requested include list.
        /// Supports: "accounts"
        /// </summary>
        protected override IQueryable<Portfolio> ApplyIncludes(IQueryable<Portfolio> query, IncludeOption[] includes)
        {
            if (includes.Contains(IncludeOption.Accounts))
            {
                query = query.Include(p => p.Accounts);
            }

            if (includes.Contains(IncludeOption.Holdings))
            {
                // If holdings are requested, ensure accounts are also included
                query = query.Include(p => p.Accounts)
                             .ThenInclude(a => a.Holdings)
                             .ThenInclude(h => h.Tags); // optional: include tags as well
            }

            if (includes.Contains(IncludeOption.Transactions))
            {
                query = query.Include(p => p.Accounts)
                             .ThenInclude(a => a.Transactions);
            }

            return query;
        }

        /// <summary>
        /// Returns all portfolios without includes.
        /// </summary>
        public override async Task<IEnumerable<Portfolio>> ListAsync(CancellationToken ct = default)
        {
            return await _dbSet.ToListAsync(ct);
        }

        /// <summary>
        /// Returns all portfolios with optional includes.
        /// </summary>
        public override async Task<IEnumerable<Portfolio>> ListWithIncludesAsync(IncludeOption[] includes, CancellationToken ct = default)
        {
            var query = ApplyIncludes(_dbSet.AsQueryable(), includes);
            return await query.ToListAsync(ct);
        }

        /// <summary>
        /// Returns a single portfolio by ID without includes.
        /// </summary>
        public override async Task<Portfolio?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        /// <summary>
        /// Returns a single portfolio by ID with optional includes.
        /// </summary>
        public override async Task<Portfolio?> GetByIdWithIncludesAsync(int id, IncludeOption[] includes, CancellationToken ct = default)
        {
            var query = ApplyIncludes(_dbSet.AsQueryable(), includes);
            return await query.FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task AddAsync(Portfolio portfolio, CancellationToken ct = default) =>
            await _dbSet.AddAsync(portfolio, ct);

        public Task UpdateAsync(Portfolio portfolio, CancellationToken ct = default)
        {
            _dbSet.Update(portfolio);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Portfolio portfolio, CancellationToken ct = default)
        {
            _dbSet.Remove(portfolio);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken ct = default) =>
            await _context.SaveChangesAsync(ct);
    }
}
