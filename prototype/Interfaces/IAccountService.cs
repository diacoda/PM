using model.Domain.Entities;
using model.Domain.Values;
using model.Repositories;

namespace model.Services;

public interface IAccountService
{
    Account Create(string name, Currency currency, FinancialInstitutions financialInstitution);
    void AddAccountToPortfolio(Portfolio portfolio, Account account);
    void RemoveAccountFromPortfolio(Portfolio portfolio, int accountId);
    Account? GetAccount(Portfolio portfolio, int accountId);
    IEnumerable<Account> ListAccounts(Portfolio portfolio);
    void UpdateAccountName(Account account, string newName);
    decimal GetCashBalance(Account account, Currency currency);
    void AddTag(Account account, Tag tag);
    void RemoveTag(Account account, Tag tag);
    IEnumerable<Holding> GetHoldingsByTag(Account account, Tag tag);
}