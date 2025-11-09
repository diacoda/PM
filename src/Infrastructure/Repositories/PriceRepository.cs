using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PM.Infrastructure.Repositories;

public class PriceRepository : IPriceRepository
{
    private readonly ValuationDbContext _db;

    public PriceRepository(ValuationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get a price for a symbol and date.
    /// </summary>
    public async Task<AssetPrice?> GetAsync(Symbol symbol, DateOnly date, CancellationToken ct = default)
    {
        return await _db.Prices
            .AsNoTracking() // read-only, avoid EF tracking conflicts
            .FirstOrDefaultAsync(p => p.Symbol.Equals(symbol) && p.Date == date, ct);
    }

    public async Task SaveAsync(AssetPrice price, CancellationToken ct = default)
    {
        _db.Prices.Add(price);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Insert or update a price. Works with immutable records.
    /// </summary>
    public async Task UpsertAsync(AssetPrice price, CancellationToken ct = default)
    {
        var existing = await _db.Prices
            .AsTracking()
            .FirstOrDefaultAsync(p => p.Symbol.Equals(price.Symbol) && p.Date == price.Date, ct);

        if (existing == null)
        {
            await _db.Prices.AddAsync(price, ct);
        }
        else
        {
            _db.Prices.Remove(existing);
            await _db.Prices.AddAsync(price, ct);
        }

        await _db.SaveChangesAsync(ct);
    }


    /// <summary>
    /// Optional helper: get all prices for a symbol
    /// </summary>
    public async Task<List<AssetPrice>> GetAllForSymbolAsync(Symbol symbol, CancellationToken ct = default)
    {
        return await _db.Prices
            .AsNoTracking()
            .Where(p => p.Symbol.Equals(symbol))
            .OrderBy(p => p.Date)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Optional helper: delete a price for a symbol and date
    /// </summary>
    public async Task<bool> DeleteAsync(Symbol symbol, DateOnly date, CancellationToken ct = default)
    {
        var existing = await _db.Prices
            .FirstOrDefaultAsync(p => p.Symbol.Equals(symbol) && p.Date == date, ct);

        if (existing is null)
            return false;

        _db.Prices.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }
    public async Task<List<AssetPrice>> GetAllByDateAsync(DateOnly date, CancellationToken ct = default)
    {
        var prices = await _db.Prices
            .AsNoTracking()
            .Where(p => p.Date == date)
            .ToListAsync(ct);

        return prices.OrderBy(p => p.Price.Amount).ToList();
    }
}
