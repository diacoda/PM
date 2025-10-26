using model.Domain.Entities;
using model.Domain.Values;
using model.Interfaces;

namespace model.Services;

/* Helpers: external flows (record in CashFlowService) vs internal events (no cash flows) */
// External flows — affect CASH and are recorded in CashFlowService so TWR neutralizes them
// Deposit Withdraw Fee
// Internal events — affect cash/quantities but NOT CashFlowService (keeps TWR clean)
// Buy Sell Dividend
public class AccountManager : IAccountManager
{
    private TransactionService _transactionService;
    private TradeCostService _costService;
    private CashFlowService _cashFlowService;
    private Instrument _cadCash = new Instrument(Symbol.From("CASH.CAD"), "Canadian Dollar", AssetClass.Cash);
    private Instrument _usdCash = new Instrument(Symbol.From("CASH.USD"), "US Dollar", AssetClass.Cash);

    public AccountManager(TransactionService transactionService, TradeCostService costService, CashFlowService cashFlowService)
    {
        _transactionService = transactionService;
        _costService = costService;
        _cashFlowService = cashFlowService;
    }

    public void Buy(Account acct, Instrument instr, decimal qty, decimal grossAmount, string ccy, DateTime d, string note = "")
    {
        var money = new Money(grossAmount, Currency.From(ccy));
        var tx = _transactionService.Create(TransactionType.Buy, instr, qty, money, d);
        tx.Costs = _costService.ComputeBuySellCost(money);
        _transactionService.AddTransaction(acct, tx, applyToCash: true);
    }
    public void Sell(Account acct, Instrument instr, decimal qty, decimal grossAmount, string ccy, DateTime d, string note = "")
    {
        var money = new Money(grossAmount, Currency.From(ccy));
        var tx = _transactionService.Create(TransactionType.Sell, instr, qty, money, d);
        tx.Costs = _costService.ComputeBuySellCost(money);
        _transactionService.AddTransaction(acct, tx, applyToCash: true);
    }
    public void Dividend(Account acct, Instrument instr, decimal amount, string ccy, DateTime d, string note = "")
    {
        var money = new Money(amount, Currency.From(ccy));
        var tx = _transactionService.Create(TransactionType.Dividend, instr, 0m, money, d);
        tx.Costs = _costService.ComputeDividendWithholding(money); // e.g., USD withholding
        _transactionService.AddTransaction(acct, tx, applyToCash: true);
    }

    public void Deposit(Account acct, decimal amt, string ccy, DateTime d, string note)
    {
        var money = new Money(amt, Currency.From(ccy));
        _transactionService.AddTransaction(acct, _transactionService.Create(TransactionType.Deposit,
            ccy == "CAD" ? _cadCash : _usdCash, 0, money, d));
        _cashFlowService.RecordCashFlow(acct, d, money, CashFlowType.Deposit, note);
    }
    public void Withdraw(Account acct, decimal amt, string ccy, DateTime d, string note)
    {
        var money = new Money(amt, Currency.From(ccy));
        _transactionService.AddTransaction(acct, _transactionService.Create(TransactionType.Withdrawal,
            ccy == "CAD" ? _cadCash : _usdCash, 0, money, d));
        _cashFlowService.RecordCashFlow(acct, d, money, CashFlowType.Withdrawal, note);
    }
    public void Fee(Account acct, decimal amt, string ccy, DateTime d, string note)
    {
        var money = new Money(amt, Currency.From(ccy));
        _transactionService.AddTransaction(acct, _transactionService.Create(TransactionType.Withdrawal,
            ccy == "CAD" ? _cadCash : _usdCash, 0, money, d));
        _cashFlowService.RecordCashFlow(acct, d, money, CashFlowType.Fee, note);
    }
}