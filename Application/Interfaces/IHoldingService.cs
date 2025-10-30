using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IHoldingService
{
    Task UpsertHoldingAsync(int accountId, Symbol symbol, decimal quantity, CancellationToken ct = default);
    Task UpdateHoldingQuantityAsync(int accountId, Symbol symbol, decimal newQty, CancellationToken ct = default);
    Task RemoveHoldingAsync(int accountId, Symbol symbol, CancellationToken ct = default);
    Task<Holding?> GetHoldingAsync(int accountId, Symbol symbol, CancellationToken ct = default);
    Task<decimal> GetCashBalanceAsync(int accountId, Currency currency, CancellationToken ct = default);
    Task<IEnumerable<Holding>> ListHoldingsAsync(int accountId, CancellationToken ct = default);
    Task AddTagAsync(int accountId, Symbol symbol, Tag tag, CancellationToken ct = default);
    Task RemoveTagAsync(int accountId, Symbol symbol, Tag tag, CancellationToken ct = default);
}