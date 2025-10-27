namespace PM.Application.Interfaces;

using PM.Domain.Entities;
using PM.Domain.Values;

/* Helpers: external flows (record in CashFlowService) vs internal events (no cash flows) */
// External flows — affect CASH and are recorded in CashFlowService so TWR neutralizes them
// Deposit Withdraw Fee
// Internal events — affect cash/quantities but NOT CashFlowService (keeps TWR clean)
// Buy Sell Dividend
public interface IAccountManager
{
    Task Buy(Account acct, Instrument instr, decimal qty, decimal grossAmount, string ccy, DateTime d, string note = "");
    Task Sell(Account acct, Instrument instr, decimal qty, decimal grossAmount, string ccy, DateTime d, string note = "");
    Task Dividend(Account acct, Instrument instr, decimal amount, string ccy, DateTime d, string note = "");
    Task Deposit(Account acct, decimal amt, string ccy, DateTime d, string note);
    Task Withdraw(Account acct, decimal amt, string ccy, DateTime d, string note);
    Task Fee(Account acct, decimal amt, string ccy, DateTime d, string note);
}