using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Application.Interfaces;
using PM.Infrastructure.Data;
using PM.Domain.Values;

namespace PM.Infrastructure.Repositories;

public class ValuationRepository : IValuationRepository
{
    private readonly ValuationDbContext _context;

    public ValuationRepository(ValuationDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(ValuationSnapshot record, CancellationToken ct = default)
    {
        foreach (var entry in _context.ChangeTracker.Entries())
        {
            entry.State = EntityState.Detached;
        }
        await _context.ValuationSnapshots.AddAsync(record, ct);
        await _context.SaveChangesAsync(ct);
    }

    public Task<IEnumerable<ValuationSnapshot>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult(_context.ValuationSnapshots.AsEnumerable());

    public Task<IEnumerable<ValuationSnapshot>> GetByPortfolioAsync(int portfolioId, ValuationPeriod period, CancellationToken ct = default)
    {
        var q = _context.ValuationSnapshots
            .Where(r => r.PortfolioId == portfolioId && r.Period == period)
            .AsEnumerable();
        return Task.FromResult(q);
    }

    public Task<IEnumerable<ValuationSnapshot>> GetByAccountAsync(int accountId, ValuationPeriod period, CancellationToken ct = default)
    {
        var q = _context.ValuationSnapshots
            .Where(r => r.AccountId == accountId && r.Period == period)
            .AsEnumerable();
        return Task.FromResult(q);
    }

    public Task<IEnumerable<ValuationSnapshot>> GetPortfolioAssetClassSnapshotsAsync(
        int portfolioId, ValuationPeriod period, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
    {
        var q = _context.ValuationSnapshots
            .Where(r => r.PortfolioId == portfolioId && r.Period == period && r.AssetClass.HasValue);

        if (from.HasValue) q = q.Where(r => r.Date >= from.Value);
        if (to.HasValue) q = q.Where(r => r.Date <= to.Value);

        return Task.FromResult(q.OrderBy(r => r.Date).ThenBy(r => r.AssetClass).AsEnumerable());
    }

    public Task<IEnumerable<ValuationSnapshot>> GetAccountAssetClassSnapshotsAsync(
        int accountId, ValuationPeriod period, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
    {
        var q = _context.ValuationSnapshots
            .Where(r => r.AccountId == accountId && r.Period == period && r.AssetClass.HasValue);

        if (from.HasValue) q = q.Where(r => r.Date >= from.Value);
        if (to.HasValue) q = q.Where(r => r.Date <= to.Value);

        return Task.FromResult(q.OrderBy(r => r.Date).ThenBy(r => r.AssetClass).AsEnumerable());
    }
    public async Task<ValuationSnapshot?> GetLatestAsync(
            EntityKind kind,
            int entityId,
            Currency reportingCurrency,
            ValuationPeriod? period = null,
            bool includeAssetClass = false,
            CancellationToken ct = default)
    {
        var query = _context.ValuationSnapshots.AsQueryable();

        query = kind switch
        {
            EntityKind.Portfolio => query.Where(v => v.PortfolioId == entityId),
            EntityKind.Account => query.Where(v => v.AccountId == entityId),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        query = query.Where(v => v.ReportingCurrency == reportingCurrency);

        if (period != null)
            query = query.Where(v => v.Period == period);

        if (!includeAssetClass)
            query = query.Where(v => v.AssetClass == null);

        return await query
            .OrderByDescending(v => v.Date)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<ValuationSnapshot>> GetRangeAsync(
        EntityKind kind,
        int entityId,
        DateOnly start,
        DateOnly end,
        Currency reportingCurrency,
        ValuationPeriod? period = null,
        AssetClass? assetClass = null,
        CancellationToken ct = default)
    {
        var query = _context.ValuationSnapshots.AsQueryable();

        query = kind switch
        {
            EntityKind.Portfolio => query.Where(v => v.PortfolioId == entityId),
            EntityKind.Account => query.Where(v => v.AccountId == entityId),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        query = query
            .Where(v => v.ReportingCurrency == reportingCurrency)
            .Where(v => v.Date >= start && v.Date <= end);

        if (period != null)
            query = query.Where(v => v.Period == period);

        if (assetClass != null)
            query = query.Where(v => v.AssetClass == assetClass);

        return await query.OrderBy(v => v.Date).ToListAsync(ct);
    }

    public async Task<IEnumerable<ValuationSnapshot>> GetAsOfDateAsync(
        EntityKind kind,
        DateOnly date,
        Currency reportingCurrency,
        ValuationPeriod? period = null,
        CancellationToken ct = default)
    {
        var query = _context.ValuationSnapshots.AsQueryable();

        query = kind switch
        {
            EntityKind.Portfolio => query.Where(v => v.PortfolioId != null),
            EntityKind.Account => query.Where(v => v.AccountId != null),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        query = query
            .Where(v => v.Date == date)
            .Where(v => v.ReportingCurrency == reportingCurrency);

        if (period != null)
            query = query.Where(v => v.Period == period);

        return await query.ToListAsync(ct);
    }
}
