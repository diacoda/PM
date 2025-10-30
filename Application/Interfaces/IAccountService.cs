using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Domain.Enums;
using PM.DTO;
using PM.SharedKernel;

namespace PM.Application.Interfaces;

public interface IAccountService
{
    Task<AccountDTO> CreateAsync(int portfolioId, string name, Currency currency, FinancialInstitutions financialInstitution, CancellationToken ct = default);
    Task RemoveAccountFromPortfolioAsync(int portfolioId, int accountId, CancellationToken ct = default);
    Task UpdateAccountNameAsync(int accountId, string newName, CancellationToken ct = default);
    Task<decimal> GetCashBalanceAsync(int accountId, Currency currency, CancellationToken ct = default);
    Task AddTagAsync(int accountId, Tag tag, CancellationToken ct = default);
    Task RemoveTagAsync(int accountId, Tag tag, CancellationToken cancellationToken = default);
    IEnumerable<HoldingDTO> GetHoldingsByTag(Account account, Tag tag);
    Task<AccountDTO?> GetAccountAsync(int portfolioId, int accountId, CancellationToken ct = default);
    Task<AccountDTO?> GetAccountWithIncludesAsync(int portfolioId, int accountId, IncludeOption[] includes, CancellationToken ct = default);
    Task<IEnumerable<AccountDTO>> ListAccountsAsync(int portfolioId, CancellationToken ct = default);
    Task<IEnumerable<AccountDTO>> ListAccountsWithIncludesAsync(int portfolioId, IncludeOption[] includes, CancellationToken ct = default);
}

