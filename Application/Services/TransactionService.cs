using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly IAccountRepository _accountRepo;

        public TransactionService(ITransactionRepository transactionRepo, IAccountRepository accountRepo)
        {
            _transactionRepo = transactionRepo;
            _accountRepo = accountRepo;
        }

        public async Task<Transaction> CreateAsync(TransactionType type, Instrument instrument, decimal quantity, Money amount, DateTime date, CancellationToken ct = default)
        {
            var tx = new Transaction
            {
                Type = type,
                Instrument = instrument,
                Quantity = quantity,
                Amount = amount,
                Date = date
            };

            await _transactionRepo.AddAsync(tx, ct);
            await _transactionRepo.SaveChangesAsync(ct);

            return tx;
        }

        public async Task AddTransactionAsync(Account account, Transaction transaction, bool applyToCash = true, CancellationToken ct = default)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            account.AddTransaction(transaction);
            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);

            // Apply to cash balance logic can go here later if Money or Ledger added
        }

        public async Task<IEnumerable<Transaction>> ListTransactionsAsync(int accountId, CancellationToken ct = default)
        {
            return await _transactionRepo.ListByAccountAsync(accountId, ct);
        }

        public async Task<Transaction?> GetTransactionAsync(int accountId, int transactionId, CancellationToken ct = default)
        {
            var transactions = await _transactionRepo.ListByAccountAsync(accountId, ct);
            return transactions.FirstOrDefault(t => t.Id == transactionId);
        }

        public async Task DeleteTransactionAsync(Account account, Guid transactionId, CancellationToken ct = default)
        {
            var tx = account.Transactions.FirstOrDefault(t => t.Id == transactionId.GetHashCode());
            if (tx == null) return;

            account.RemoveTransaction(tx);
            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);
        }
    }
}
