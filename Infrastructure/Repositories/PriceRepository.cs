using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolio.Infrastructure.Repositories;

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
    public async Task<InstrumentPrice?> GetAsync(Symbol symbol, DateOnly date)
    {
        return await _db.Prices
            .AsNoTracking() // read-only, avoid EF tracking conflicts
            .FirstOrDefaultAsync(p => p.Symbol.Equals(symbol) && p.Date == date);
    }

    public async Task SaveAsync(InstrumentPrice price)
    {
        _db.Prices.Add(price);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Insert or update a price. Works with immutable records.
    /// </summary>
    public async Task UpsertAsync(InstrumentPrice price)
    {
        // Look for existing row
        var existing = await _db.Prices
            .AsTracking() // tracking needed to update
            .FirstOrDefaultAsync(p => p.Symbol.Equals(price.Symbol) && p.Date == price.Date);

        if (existing != null)
        {
            // Update tracked entity with new values
            _db.Entry(existing).CurrentValues.SetValues(price);
        }
        else
        {
            await _db.Prices.AddAsync(price);
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Optional helper: get all prices for a symbol
    /// </summary>
    public async Task<List<InstrumentPrice>> GetAllForSymbolAsync(Symbol symbol)
    {
        return await _db.Prices
            .AsNoTracking()
            .Where(p => p.Symbol.Equals(symbol))
            .OrderBy(p => p.Date)
            .ToListAsync();
    }

    /// <summary>
    /// Optional helper: delete a price for a symbol and date
    /// </summary>
    public async Task<bool> DeleteAsync(Symbol symbol, DateOnly date)
    {
        var existing = await _db.Prices
            .FirstOrDefaultAsync(p => p.Symbol.Equals(symbol) && p.Date == date);

        if (existing is null)
            return false;

        _db.Prices.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
    public async Task<List<InstrumentPrice>> GetAllByDateAsync(DateOnly date)
    {
        return await _db.Prices
            .AsNoTracking()
            .Where(p => p.Date == date)
            .OrderBy(p => (double)p.Price.Amount)  // 👈 convert decimal to double for SQLite compatibility
            .ToListAsync();
    }
}
