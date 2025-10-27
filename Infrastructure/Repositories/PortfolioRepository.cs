using Microsoft.EntityFrameworkCore;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Infrastructure.Data;

namespace PM.Infrastructure.Repositories
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly AppDbContext _db;

        public PortfolioRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Portfolio?> GetByIdAsync(int id, CancellationToken ct = default) =>
            await _db.Portfolios
                     .Include(p => p.Accounts)
                     .ThenInclude(a => a.Holdings)
                     .FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<IEnumerable<Portfolio>> ListAsync(CancellationToken ct = default) =>
            await _db.Portfolios.Include(p => p.Accounts).ToListAsync(ct);

        public async Task AddAsync(Portfolio portfolio, CancellationToken ct = default) =>
            await _db.Portfolios.AddAsync(portfolio, ct);

        public Task UpdateAsync(Portfolio portfolio, CancellationToken ct = default)
        {
            _db.Portfolios.Update(portfolio);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Portfolio portfolio, CancellationToken ct = default)
        {
            _db.Portfolios.Remove(portfolio);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken ct = default) =>
            await _db.SaveChangesAsync(ct);
    }
}
