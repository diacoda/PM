using Microsoft.EntityFrameworkCore;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Infrastructure.Data;
using PM.SharedKernel;

namespace PM.Infrastructure.Repositories;

public class AccountRepository : BaseRepository<Account>, IAccountRepository
{
    public AccountRepository(PortfolioDbContext db) : base(db) { }

    /// <summary>
    /// Applies conditional includes based on the requested include list.
    /// Supports: "holdings", "transactions", "tags"
    /// </summary>
    protected override IQueryable<Account> ApplyIncludes(IQueryable<Account> query, IncludeOption[] includes)
    {
        if (includes.Contains(IncludeOption.Holdings))
            query = query.Include(a => a.Holdings);

        if (includes.Contains(IncludeOption.Transactions))
            query = query.Include(a => a.Transactions);

        if (includes.Contains(IncludeOption.Tags))
            query = query.Include(a => a.Tags);

        return query.AsSplitQuery();
    }

    /// <summary>
    /// Returns all accounts for a given portfolio (no includes).
    /// </summary>
    public async Task<IEnumerable<Account>> ListByPortfolioAsync(int portfolioId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(a => a.PortfolioId == portfolioId)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Returns all accounts for a given portfolio with optional includes.
    /// </summary>
    public async Task<IEnumerable<Account>> ListByPortfolioWithIncludesAsync(int portfolioId, IncludeOption[] includes, CancellationToken ct = default)
    {
        var query = ApplyIncludes(_dbSet.AsQueryable(), includes)
            .Where(a => a.PortfolioId == portfolioId);

        return await query.ToListAsync(ct);
    }

    /// <summary>
    /// Adds a new account.
    /// </summary>
    public async Task AddAsync(Account account, CancellationToken ct = default) =>
        await _dbSet.AddAsync(account, ct);

    /// <summary>
    /// Updates an existing account.
    /// </summary>
    public Task UpdateAsync(Account account, CancellationToken ct = default)
    {
        _dbSet.Update(account);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes an account.
    /// </summary>
    public Task DeleteAsync(Account account, CancellationToken ct = default)
    {
        _dbSet.Remove(account);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Persists changes to the database.
    /// </summary>
    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}