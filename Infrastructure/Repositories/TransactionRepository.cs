using Microsoft.EntityFrameworkCore;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Infrastructure.Data;

namespace PM.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _db;
        public TransactionRepository(AppDbContext db) => _db = db;

        public async Task<Transaction?> GetByIdAsync(int id, CancellationToken ct = default) =>
            await _db.Transactions.FirstOrDefaultAsync(t => t.Id == id, ct);

        public async Task<IEnumerable<Transaction>> ListByAccountAsync(int accountId, CancellationToken ct = default) =>
            await _db.Transactions.Where(t => t.AccountId == accountId).ToListAsync(ct);

        public async Task AddAsync(Transaction transaction, CancellationToken ct = default) =>
            await _db.Transactions.AddAsync(transaction, ct);

        public Task DeleteAsync(Transaction transaction, CancellationToken ct = default)
        {
            _db.Transactions.Remove(transaction);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken ct = default) =>
            await _db.SaveChangesAsync(ct);
    }
}
