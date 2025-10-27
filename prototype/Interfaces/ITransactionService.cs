using model.Domain.Entities;
using model.Domain.Values;

namespace model.Interfaces;

public interface ITransactionService
{
    Transaction Create(TransactionType type, Instrument instrument, decimal quantity, Money amount, DateTime date);
    void AddTransaction(Account account, Transaction transaction, bool applyToCash = true);
    IEnumerable<Transaction> ListTransactions(Account account);
    Transaction? GetTransaction(Account account, Guid transactionId);
    void DeleteTransaction(Account account, Guid transactionId);
}
