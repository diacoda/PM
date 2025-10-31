using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface ITransactionService
{
    Task<Transaction> CreateAsync(Transaction tx, CancellationToken ct = default);
    Task<IEnumerable<Transaction>> ListTransactionsAsync(int acountId, CancellationToken ct = default);
    Task<Transaction?> GetTransactionAsync(int acountId, int transactionId, CancellationToken ct = default);
    Task DeleteTransactionAsync(Account account, int transactionId, CancellationToken ct = default);
}
