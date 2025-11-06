using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.SharedKernel;

namespace PM.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly IAccountRepository _accountRepo;

        public TransactionService(
            ITransactionRepository transactionRepo,
            IAccountRepository accountRepo)
        {
            _transactionRepo = transactionRepo;
            _accountRepo = accountRepo;
        }

        public async Task<Transaction> CreateAsync(
            Transaction tx,
            CancellationToken ct = default)
        {
            await _transactionRepo.AddAsync(tx, ct);
            await _transactionRepo.SaveChangesAsync(ct);

            return tx;
        }

        public async Task<IEnumerable<Transaction>> ListTransactionsAsync(
            int accountId,
            CancellationToken ct = default)
        {
            return await _transactionRepo.ListByAccountAsync(accountId, ct);
        }

        public async Task<Transaction?> GetTransactionAsync(
            int accountId,
            int transactionId,
            CancellationToken ct = default)
        {
            var transactions = await _transactionRepo.ListByAccountAsync(accountId, ct);
            return transactions.FirstOrDefault(t => t.Id == transactionId);
        }

        public async Task DeleteTransactionAsync(
            Account account,
            int transactionId,
            CancellationToken ct = default)
        {
            var tx = account.Transactions.FirstOrDefault(t => t.Id == transactionId);
            if (tx == null) return;

            account.RemoveTransaction(tx);
            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);
        }
    }
}
