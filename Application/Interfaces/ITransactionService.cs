using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface ITransactionService
{
    Task<Transaction> CreateAsync(TransactionType type, Symbol instrument, decimal quantity, Money amount, DateTime date, CancellationToken ct = default);
    Task AddTransactionAsync(Account account, Transaction transaction, bool applyToCash = true, CancellationToken ct = default);
    Task<IEnumerable<Transaction>> ListTransactionsAsync(int acountId, CancellationToken ct = default);
    Task<Transaction?> GetTransactionAsync(int acountId, int transactionId, CancellationToken ct = default);
    Task DeleteTransactionAsync(Account account, Guid transactionId, CancellationToken ct = default);
}
