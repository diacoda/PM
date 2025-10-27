using model.Domain.Entities;
using model.Domain.Values;
using model.Interfaces;

namespace model.Services;

public class TransactionService : ITransactionService
{
    /// <summary>
    /// Factory to build a transaction record (no side effects). Set Costs later if needed.
    /// </summary>
    public Transaction Create(TransactionType type, Instrument instrument, decimal quantity, Money amount, DateTime date)
        => new Transaction
        {
            Type = type,
            Instrument = instrument,
            Quantity = quantity,
            Amount = amount,
            Date = date
        };

    /// <summary>
    /// Adds a transaction to the account and applies effects to holdings and cash.
    /// Policy:
    ///  - Deposits / Withdrawals: adjust CASH.* holdings only (external flows).
    ///  - Buy: increase position qty; decrease CASH by (Amount + Costs).
    ///  - Sell: decrease position qty (remove if 0); increase CASH by (Amount - Costs).
    ///  - Dividend: increase CASH by (Amount - Costs); position unchanged.
    /// NOTE: Do NOT record Buy/Sell/Dividend in CashFlowService (internal), so TWR remains clean.
    /// </summary>
    public void AddTransaction(Account account, Transaction transaction, bool applyToCash = true)
    {
        //account.Transactions.Add(transaction);
        account.AddTransaction(transaction);

        Holding EnsureCash(Currency ccy)
        {
            var sym = Symbol.From($"CASH.{ccy}");
            var h = account.Holdings.FirstOrDefault(x => x.Instrument.Symbol == sym);
            if (h == null)
            {
                h = new Holding { Instrument = new Instrument(sym, $"{ccy} Cash", AssetClass.Cash), Quantity = 0m };
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
                        pos = new Holding { Instrument = transaction.Instrument, Quantity = 0m };
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
    }

    public IEnumerable<Transaction> ListTransactions(Account account)
        => account.Transactions.OrderByDescending(t => t.Date);

    public Transaction? GetTransaction(Account account, Guid transactionId)
        => account.Transactions.FirstOrDefault(t => t.Id == transactionId);

    public void DeleteTransaction(Account account, Guid transactionId)
    {
        var tx = GetTransaction(account, transactionId);
        if (tx != null)
        {
            //account.Transactions.Remove(tx);
            account.RemoveTransaction(tx);
            // KISS: no automatic reversal of side effects here.
        }
    }
}
