using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Domain.Enums;

namespace PM.Application.Interfaces;

public interface IAccountService
{
    //Task<Account> CreateAsync(string name, Currency currency, FinancialInstitutions financialInstitution, CancellationToken ct = default);
    Task<Account> CreateAsync(int portfolioId, string name, Currency currency, FinancialInstitutions financialInstitution, CancellationToken ct = default);

    //Task AddAccountToPortfolioAsync(int portfolioId, int accountId, CancellationToken ct = default);

    Task RemoveAccountFromPortfolioAsync(int portfolioId, int accountId, CancellationToken ct = default);

    Task<Account?> GetAccountAsync(int portfolioId, int accountId, CancellationToken ct = default);

    Task<IEnumerable<Account>> ListAccountsAsync(int portfolioId, CancellationToken ct = default);

    Task UpdateAccountNameAsync(int accountId, string newName, CancellationToken ct = default);

    Task<decimal> GetCashBalanceAsync(int accountId, Currency currency, CancellationToken ct = default);

    Task AddTagAsync(int accountId, Tag tag, CancellationToken ct = default);

    Task RemoveTagAsync(int accountId, Tag tag, CancellationToken cancellationToken = default);

    public IEnumerable<Holding> GetHoldingsByTag(Account account, Tag tag);
}

