namespace PM.Application.Interfaces;

using PM.Domain.Entities;
using PM.Domain.Values;

public interface IPriceRepository
{
    Task<InstrumentPrice?> GetAsync(Symbol symbol, DateOnly date);
    Task SaveAsync(InstrumentPrice price);
    Task UpsertAsync(InstrumentPrice price);
    Task<List<InstrumentPrice>> GetAllForSymbolAsync(Symbol symbol);
    Task<bool> DeleteAsync(Symbol symbol, DateOnly date);
    Task<List<InstrumentPrice>> GetAllByDateAsync(DateOnly date);
}