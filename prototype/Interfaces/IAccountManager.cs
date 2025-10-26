namespace model.Interfaces;

using model.Domain.Entities;
using model.Domain.Values;

/* Helpers: external flows (record in CashFlowService) vs internal events (no cash flows) */
// External flows — affect CASH and are recorded in CashFlowService so TWR neutralizes them
// Deposit Withdraw Fee
// Internal events — affect cash/quantities but NOT CashFlowService (keeps TWR clean)
// Buy Sell Dividend
public interface IAccountManager
{
    public void Buy(Account acct, Instrument instr, decimal qty, decimal grossAmount, string ccy, DateTime d, string note = "");
    public void Sell(Account acct, Instrument instr, decimal qty, decimal grossAmount, string ccy, DateTime d, string note = "");
    public void Dividend(Account acct, Instrument instr, decimal amount, string ccy, DateTime d, string note = "");
    public void Deposit(Account acct, decimal amt, string ccy, DateTime d, string note);
    public void Withdraw(Account acct, decimal amt, string ccy, DateTime d, string note);
    public void Fee(Account acct, decimal amt, string ccy, DateTime d, string note);
}