using Microsoft.EntityFrameworkCore;
using PM.Application.Interfaces;
using PM.Domain.Entities;

namespace PM.Infrastructure.Data.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _db;
        public AccountRepository(AppDbContext db) => _db = db;

        public async Task<Account?> GetByIdAsync(int id, CancellationToken ct = default) =>
            await _db.Accounts
                     .Include(a => a.Holdings)
                     .Include(a => a.Transactions)
                     .Include(a => a.Tags)
                     .FirstOrDefaultAsync(a => a.Id == id, ct);

        public async Task<IEnumerable<Account>> ListByPortfolioAsync(int portfolioId, CancellationToken ct = default)
        {
            var portfolio = await _db.Portfolios
                .Include(p => p.Accounts)
                .ThenInclude(a => a.Holdings)
                .FirstOrDefaultAsync(p => p.Id == portfolioId, ct);

            return portfolio?.Accounts ?? Enumerable.Empty<Account>();
        }

        public async Task AddAsync(Account account, CancellationToken ct = default) =>
            await _db.Accounts.AddAsync(account, ct);

        public Task UpdateAsync(Account account, CancellationToken ct = default)
        {
            _db.Accounts.Update(account);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Account account, CancellationToken ct = default)
        {
            _db.Accounts.Remove(account);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken ct = default) =>
            await _db.SaveChangesAsync(ct);
    }
}
