using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Interfaces;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IHoldingService
{
    Task<Holding> UpsertHoldingAsync(int accountId, IAsset asset, decimal quantity, CancellationToken ct = default);
    Task<Holding> UpdateHoldingQuantityAsync(int accountId, IAsset asset, decimal newQty, CancellationToken ct = default);
    Task RemoveHoldingAsync(int accountId, IAsset asset, CancellationToken ct = default);
    Task<Holding?> GetHoldingAsync(int accountId, IAsset asset, CancellationToken ct = default);
    Task<decimal> GetCashBalanceAsync(int accountId, Currency currency, CancellationToken ct = default);
    Task<IEnumerable<Holding>> ListHoldingsAsync(int accountId, CancellationToken ct = default);
    Task AddTagAsync(int accountId, IAsset asset, Tag tag, CancellationToken ct = default);
    Task RemoveTagAsync(int accountId, IAsset asset, Tag tag, CancellationToken ct = default);
}