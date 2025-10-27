using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Application.Interfaces;
using PM.Infrastructure.Data;

namespace PM.Infrastructure.Repositories;

public class ValuationRepository : IValuationRepository
{
    private readonly ValuationDbContext _context;

    public ValuationRepository(ValuationDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(ValuationRecord record)
    {
        _context.ValuationRecords.Add(record);
        await _context.SaveChangesAsync();
    }

    public Task<IEnumerable<ValuationRecord>> GetAllAsync()
        => Task.FromResult(_context.ValuationRecords.AsEnumerable());

    public Task<IEnumerable<ValuationRecord>> GetByPortfolioAsync(int portfolioId, ValuationPeriod period)
    {
        var q = _context.ValuationRecords
            .Where(r => r.PortfolioId == portfolioId && r.Period == period)
            .AsEnumerable();
        return Task.FromResult(q);
    }

    public Task<IEnumerable<ValuationRecord>> GetByAccountAsync(int accountId, ValuationPeriod period)
    {
        var q = _context.ValuationRecords
            .Where(r => r.AccountId == accountId && r.Period == period)
            .AsEnumerable();
        return Task.FromResult(q);
    }

    public Task<IEnumerable<ValuationRecord>> GetPortfolioAssetClassSnapshotsAsync(
        int portfolioId, ValuationPeriod period, DateTime? from = null, DateTime? to = null)
    {
        var q = _context.ValuationRecords
            .Where(r => r.PortfolioId == portfolioId && r.Period == period && r.AssetClass.HasValue);

        if (from.HasValue) q = q.Where(r => r.Date.Date >= from.Value.Date);
        if (to.HasValue) q = q.Where(r => r.Date.Date <= to.Value.Date);

        return Task.FromResult(q.OrderBy(r => r.Date).ThenBy(r => r.AssetClass).AsEnumerable());
    }

    public Task<IEnumerable<ValuationRecord>> GetAccountAssetClassSnapshotsAsync(
        int accountId, ValuationPeriod period, DateTime? from = null, DateTime? to = null)
    {
        var q = _context.ValuationRecords
            .Where(r => r.AccountId == accountId && r.Period == period && r.AssetClass.HasValue);

        if (from.HasValue) q = q.Where(r => r.Date.Date >= from.Value.Date);
        if (to.HasValue) q = q.Where(r => r.Date.Date <= to.Value.Date);

        return Task.FromResult(q.OrderBy(r => r.Date).ThenBy(r => r.AssetClass).AsEnumerable());
    }
}
