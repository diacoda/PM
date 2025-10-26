using model.Domain.Entities;
using model.Domain.Values;
using model.Repositories;

namespace model.Services;

public class AccountService
{
    public Account Create(string name, Currency currency, FinancialInstitutions financialInstitution)
    {
        return new Account() { Name = name, Currency = currency, FinancialInstitution = financialInstitution };
    }

    public void AddAccountToPortfolio(Portfolio portfolio, Account account)
    {
        portfolio.Accounts.Add(account);
    }

    public void RemoveAccountFromPortfolio(Portfolio portfolio, int accountId)
    {
        var account = portfolio.Accounts.FirstOrDefault(a => a.Id == accountId);
        if (account != null)
        {
            portfolio.Accounts.Remove(account);
        }
    }

    public Account? GetAccount(Portfolio portfolio, int accountId)
    {
        return portfolio.Accounts.FirstOrDefault(a => a.Id == accountId);
    }

    public IEnumerable<Account> ListAccounts(Portfolio portfolio)
    {
        return portfolio.Accounts;
    }

    public void UpdateAccountName(Account account, string newName)
    {
        account.Name = newName;
    }

    public decimal GetCashBalance(Account account, Currency currency)
    {
        var symbol = Symbol.From($"CASH.{currency}");
        var holding = account.Holdings.FirstOrDefault(h => h.Instrument.Symbol == symbol);
        return holding?.Quantity ?? 0;
    }

    public void AddTag(Account account, Tag tag)
    {
        if (!account.Tags.Contains(tag))
        {
            //account.Tags.Add(tag);
            account.AddTag(tag);
        }
    }

    public void RemoveTag(Account account, Tag tag)
    {
        account.RemoveTag(tag);
        //account.Tags.Remove(tag);
    }

    public IEnumerable<Holding> GetHoldingsByTag(Account account, Tag tag)
    {
        return account.Holdings.Where(h => h.Tags.Contains(tag));
    }
}