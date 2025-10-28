using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IHoldingService
{
    Task AddHoldingAsync(int accountId, Symbol symbol, decimal quantity, CancellationToken ct = default);

    Task RemoveHoldingAsync(int accountId, Symbol symbol, CancellationToken ct = default);

    Task<Holding?> GetHoldingAsync(int accountId, Symbol symbol, CancellationToken ct = default);

    Task UpdateHoldingQuantityAsync(Holding holding, decimal newQty, CancellationToken ct = default);

    Task<decimal> GetCashBalanceAsync(int accountId, Currency currency, CancellationToken ct = default);

    Task<IEnumerable<Holding>> ListHoldingsAsync(int accountId, CancellationToken ct = default);

    Task AddTagAsync(int holdingId, Tag tag, CancellationToken ct = default);

    Task RemoveTagAsync(int holdingId, Tag tag, CancellationToken ct = default);
}
