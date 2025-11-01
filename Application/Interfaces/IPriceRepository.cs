namespace PM.Application.Interfaces;

using PM.Domain.Entities;
using PM.Domain.Values;

public interface IPriceRepository
{
    Task<InstrumentPrice?> GetAsync(Symbol symbol, DateOnly date, CancellationToken ct = default);
    Task SaveAsync(InstrumentPrice price, CancellationToken ct = default);
    Task UpsertAsync(InstrumentPrice price, CancellationToken ct = default);
    Task<List<InstrumentPrice>> GetAllForSymbolAsync(Symbol symbol, CancellationToken ct = default);
    Task<bool> DeleteAsync(Symbol symbol, DateOnly date, CancellationToken ct = default);
    Task<List<InstrumentPrice>> GetAllByDateAsync(DateOnly date, CancellationToken ct = default);
}