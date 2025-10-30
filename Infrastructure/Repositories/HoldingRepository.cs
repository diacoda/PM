using Microsoft.EntityFrameworkCore;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Infrastructure.Data;

namespace PM.Infrastructure.Repositories;

public class HoldingRepository : IHoldingRepository
{
    private readonly AppDbContext _db;
    public HoldingRepository(AppDbContext db) => _db = db;

    public async Task<Holding?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.Holdings.Include(h => h.Tags).FirstOrDefaultAsync(h => h.Id == id, ct);

    public async Task<IEnumerable<Holding>> ListByAccountAsync(int accountId, CancellationToken ct = default) =>
        await _db.Holdings.Where(h => h.AccountId == accountId).ToListAsync(ct);

    public Task UpdateAsync(Holding holding, CancellationToken ct = default)
    {
        _db.Holdings.Update(holding);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Holding holding, CancellationToken ct = default)
    {
        _db.Holdings.Remove(holding);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
