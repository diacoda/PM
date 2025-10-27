using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Services
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

            Holding EnsureCash(Currency ccy)
            {
                var sym = Symbol.From($"CASH.{ccy}");
                var h = account.Holdings.FirstOrDefault(x => x.Instrument.Symbol == sym);
                if (h == null)
                {
                    h = new Holding(new Instrument(sym, $"{ccy} Cash", AssetClass.Cash), 0m);
                    //account.Holdings.Add(h);
                    account.AddHolding(h);
                }
                return h;
            }

            Holding? FindPosition() =>
                account.Holdings.FirstOrDefault(h => h.Instrument.Symbol == transaction.Instrument.Symbol);

            decimal CostOrZero() => transaction.Costs?.Amount ?? 0m;

            switch (transaction.Type)
            {
                case TransactionType.Deposit:
                    if (applyToCash)
                    {
                        var cash = EnsureCash(transaction.Amount.Currency);
                        cash.Quantity += transaction.Amount.Amount;
                    }
                    break;

                case TransactionType.Withdrawal:
                    if (applyToCash)
                    {
                        var cash = EnsureCash(transaction.Amount.Currency);
                        cash.Quantity -= transaction.Amount.Amount;
                    }
                    break;

                case TransactionType.Buy:
                    {
                        var pos = FindPosition();
                        if (pos == null)
                        {
                            pos = new Holding(transaction.Instrument, 0m);
                            //account.Holdings.Add(pos);
                            account.AddHolding(pos);
                        }
                        pos.Quantity += transaction.Quantity;

                        if (applyToCash)
                        {
                            var cash = EnsureCash(transaction.Amount.Currency);
                            cash.Quantity -= (transaction.Amount.Amount + CostOrZero());
                        }
                        break;
                    }

                case TransactionType.Sell:
                    {
                        var pos = FindPosition();
                        if (pos == null) break; // KISS: ignore invalid sell

                        pos.Quantity -= transaction.Quantity;
                        if (pos.Quantity <= 0m)
                        {
                            pos.Quantity = 0m;
                            //account.Holdings.Remove(pos);
                            account.RemoveHolding(pos);
                        }

                        if (applyToCash)
                        {
                            var cash = EnsureCash(transaction.Amount.Currency);
                            cash.Quantity += (transaction.Amount.Amount - CostOrZero());
                        }
                        break;
                    }

                case TransactionType.Dividend:
                    {
                        if (applyToCash)
                        {
                            var cash = EnsureCash(transaction.Amount.Currency);
                            cash.Quantity += (transaction.Amount.Amount - CostOrZero()); // net of withholding
                        }
                        break;
                    }

                default:
                    // Other types ignored in this KISS version.
                    break;
            }
            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);
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
