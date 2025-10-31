using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Application.Interfaces;
using PM.Domain.Enums;

namespace PM.Application.Services;

/* Helpers: external flows (record in CashFlowService) vs internal events (no cash flows) */
// External flows — affect CASH and are recorded in CashFlowService so TWR neutralizes them
// Deposit Withdraw Fee
// Internal events — affect cash/quantities but NOT CashFlowService (keeps TWR clean)
// Buy Sell Dividend
public class AccountManager : IAccountManager
{
    private ITransactionService _transactionService;
    private ITradeCostService _costService;
    private ICashFlowService _cashFlowService;
    private Symbol _cadCash = new Symbol("CAD");
    private Symbol _usdCash = new Symbol("USD", "USD");

    public AccountManager(ITransactionService transactionService, ITradeCostService costService, ICashFlowService cashFlowService)
    {
        _transactionService = transactionService;
        _costService = costService;
        _cashFlowService = cashFlowService;
    }

    public async Task Buy(Account acct, Symbol instr, decimal qty, decimal grossAmount, string ccy, DateTime d, string note = "")
    {
        var money = new Money(grossAmount, new Currency(ccy));
        //var tx = await _transactionService.CreateAsync2(acct.Id,TransactionType.Buy, instr, qty, money, d);
        //tx.Costs = _costService.ComputeBuySellCost(money);
        //await _transactionService.AddTransactionAsync(acct.Id, tx, applyToCash: true);
    }
    public async Task Sell(Account acct, Symbol instr, decimal qty, decimal grossAmount, string ccy, DateTime d, string note = "")
    {
        var money = new Money(grossAmount, new Currency(ccy));
        //var tx = await _transactionService.CreateAsync2(acct.Id,TransactionType.Sell, instr, qty, money, d);
        //tx.Costs = _costService.ComputeBuySellCost(money);
        //await _transactionService.AddTransactionAsync(acct.Id, tx, applyToCash: true);
    }
    public async Task Dividend(int accountId, Symbol instr, decimal amount, string ccy, DateTime d, string note = "")
    {
        var money = new Money(amount, new Currency(ccy));
        //var tx = await _transactionService.CreateAsync2(accountId, TransactionType.Dividend, instr, 0m, money, d);
        //tx.Costs = _costService.ComputeDividendWithholding(money); // e.g., USD withholding
        //await _transactionService.AddTransactionAsync(accountId, tx, applyToCash: true);
    }

    public async Task Deposit(int accountId, decimal amt, string ccy, DateTime d, string note)
    {
        var money = new Money(amt, new Currency(ccy));
        //Transaction tx = await _transactionService.CreateAsync2(accountId,TransactionType.Deposit,
        //    ccy == "CAD" ? _cadCash : _usdCash, 0, money, d);
        //await _transactionService.AddTransactionAsync(accountId, tx);
        await _cashFlowService.RecordCashFlowAsync(accountId, d, money, CashFlowType.Deposit, note);
    }
    public async Task Withdraw(int accountId, decimal amt, string ccy, DateTime d, string note)
    {
        var money = new Money(amt, new Currency(ccy));
        //Transaction tx = await _transactionService.CreateAsync2(accountId, TransactionType.Withdrawal,
        //    ccy == "CAD" ? _cadCash : _usdCash, 0, money, d);
        //await _transactionService.AddTransactionAsync(accountId, tx);
        await _cashFlowService.RecordCashFlowAsync(accountId, d, money, CashFlowType.Withdrawal, note);
    }
    public async Task Fee(int accountId, decimal amt, string ccy, DateTime d, string note)
    {
        var money = new Money(amt, new Currency(ccy));
        //Transaction tx = await _transactionService.CreateAsync2(accountId, TransactionType.Withdrawal,
        //    ccy == "CAD" ? _cadCash : _usdCash, 0, money, d);
        //await _transactionService.AddTransactionAsync(accountId, tx);
        await _cashFlowService.RecordCashFlowAsync(accountId, d, money, CashFlowType.Fee, note);
    }
}