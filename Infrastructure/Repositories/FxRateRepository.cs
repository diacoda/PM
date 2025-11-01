using PM.Application.Interfaces;
using PM.Domain.Values;
using PM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PM.Infrastructure.Repositories;

public class FxRateRepository : IFxRateRepository
{
    private readonly ValuationDbContext _db;

    public FxRateRepository(ValuationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get FX rate for a currency pair and date
    /// </summary>
    public async Task<FxRate?> GetAsync(Currency fromCurrency, Currency toCurrency, DateOnly date, CancellationToken ct = default)
    {
        return await _db.FxRates
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FromCurrency.Equals(fromCurrency)
                                   && f.ToCurrency.Equals(toCurrency)
                                   && f.Date == date);
    }

    /// <summary>
    /// Save a new FX rate
    /// </summary>
    public async Task SaveAsync(FxRate rate, CancellationToken ct = default)
    {
        _db.FxRates.Add(rate);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Insert or update a FX rate
    /// </summary>
    public async Task UpsertAsync(FxRate rate, CancellationToken ct = default)
    {
        var existing = await _db.FxRates
            .AsTracking()
            .FirstOrDefaultAsync(f => f.FromCurrency.Equals(rate.FromCurrency)
                                   && f.ToCurrency.Equals(rate.ToCurrency)
                                   && f.Date == rate.Date);

        if (existing != null)
        {
            _db.Entry(existing).CurrentValues.SetValues(rate);
        }
        else
        {
            await _db.FxRates.AddAsync(rate);
        }

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Get all FX rates for a currency pair, ordered by date
    /// </summary>
    public async Task<List<FxRate>> GetAllForPairAsync(Currency fromCurrency, Currency toCurrency, CancellationToken ct = default)
    {
        return await _db.FxRates
            .AsNoTracking()
            .Where(f => f.FromCurrency.Equals(fromCurrency) && f.ToCurrency.Equals(toCurrency))
            .OrderBy(f => f.Date)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Delete a FX rate for a currency pair and date
    /// </summary>
    public async Task<bool> DeleteAsync(Currency fromCurrency, Currency toCurrency, DateOnly date, CancellationToken ct = default)
    {
        var existing = await _db.FxRates
            .FirstOrDefaultAsync(f => f.FromCurrency.Equals(fromCurrency)
                                   && f.ToCurrency.Equals(toCurrency)
                                   && f.Date == date);

        if (existing == null)
            return false;

        _db.FxRates.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>
    /// Get all FX rates for a specific date, ordered by FromCurrency / ToCurrency
    /// </summary>
    public async Task<List<FxRate>> GetAllByDateAsync(DateOnly date, CancellationToken ct = default)
    {
        var rates = await _db.FxRates
            .AsNoTracking()
            .Where(f => f.Date == date)
            .ToListAsync(ct);

        return rates
            .OrderBy(f => f.FromCurrency.Code)
            .ThenBy(f => f.ToCurrency.Code)
            .ToList();
    }
}
