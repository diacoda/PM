namespace PM.Application.Interfaces;

using PM.Domain.Entities;
using PM.Domain.Values;

public interface IPriceRepository
{
    Task<AssetPrice?> GetAsync(Symbol symbol, DateOnly date, CancellationToken ct = default);
    Task SaveAsync(AssetPrice price, CancellationToken ct = default);
    Task UpsertAsync(AssetPrice price, CancellationToken ct = default);
    Task<List<AssetPrice>> GetAllForSymbolAsync(Symbol symbol, CancellationToken ct = default);
    Task<bool> DeleteAsync(Symbol symbol, DateOnly date, CancellationToken ct = default);
    Task<List<AssetPrice>> GetAllByDateAsync(DateOnly date, CancellationToken ct = default);
}