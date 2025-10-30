using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.Application.Interfaces;
using PM.DTO;
using PM.Domain.Mappers;
using PM.SharedKernel;

namespace PM.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPortfolioRepository _portfolioRepository;

    public AccountService(
        IAccountRepository accountRepository,
        IPortfolioRepository portfolioRepository)
    {
        _accountRepository = accountRepository;
        _portfolioRepository = portfolioRepository;
    }

    public async Task<AccountDTO> CreateAsync(
        int portfolioId,
        string name,
        Currency currency,
        FinancialInstitutions financialInstitution,
        CancellationToken ct = default)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(portfolioId, ct)
            ?? throw new KeyNotFoundException($"Portfolio with ID {portfolioId} not found.");

        var account = new Account(name, currency, financialInstitution);
        account.LinkToPortfolio(portfolio);

        await _accountRepository.AddAsync(account, ct);
        await _accountRepository.SaveChangesAsync(ct);

        return AccountMapper.ToDTO(account);
    }

    public async Task<AccountDTO?> GetAccountAsync(int portfolioId, int accountId, CancellationToken ct = default)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, ct);
        if (account == null || account.PortfolioId != portfolioId)
            return null;

        return AccountMapper.ToDTO(account);
    }

    public async Task<AccountDTO?> GetAccountWithIncludesAsync(int portfolioId, int accountId, IncludeOption[] includes, CancellationToken ct = default)
    {
        var account = await _accountRepository.GetByIdWithIncludesAsync(accountId, includes, ct);
        if (account == null || account.PortfolioId != portfolioId)
            return null;

        return AccountMapper.ToDTO(account, includes);
    }

    public async Task<IEnumerable<AccountDTO>> ListAccountsAsync(int portfolioId, CancellationToken ct = default)
    {
        var accounts = await _accountRepository.ListByPortfolioAsync(portfolioId, ct);
        return accounts.Select(AccountMapper.ToDTO).ToList();
    }

    public async Task<IEnumerable<AccountDTO>> ListAccountsWithIncludesAsync(int portfolioId, IncludeOption[] includes, CancellationToken ct = default)
    {
        var accounts = await _accountRepository.ListByPortfolioWithIncludesAsync(portfolioId, includes, ct);
        return accounts.Select(a => AccountMapper.ToDTO(a, includes)).ToList();
    }

    public async Task UpdateAccountNameAsync(int accountId, string newName, CancellationToken ct = default)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, ct)
            ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");

        account.UpdateName(newName);
        await _accountRepository.SaveChangesAsync(ct);
    }

    public async Task<decimal> GetCashBalanceAsync(int accountId, Currency currency, CancellationToken ct = default)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, ct)
            ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");

        return account.GetCashBalance(currency);
    }

    public async Task AddTagAsync(int accountId, Tag tag, CancellationToken ct = default)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, ct)
            ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");

        account.AddTag(tag);
        await _accountRepository.SaveChangesAsync(ct);
    }

    public async Task RemoveTagAsync(int accountId, Tag tag, CancellationToken ct = default)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, ct)
            ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");

        account.RemoveTag(tag);
        await _accountRepository.SaveChangesAsync(ct);
    }

    public IEnumerable<HoldingDTO> GetHoldingsByTag(Account account, Tag tag)
    {
        return account.Holdings
            .Where(h => h.Tags.Contains(tag))
            .Select(HoldingMapper.ToDTO)
            .ToList();
    }

    public async Task RemoveAccountFromPortfolioAsync(int portfolioId, int accountId, CancellationToken ct = default)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, ct)
            ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");

        if (account.PortfolioId != portfolioId)
            throw new InvalidOperationException($"Account {accountId} is not linked to Portfolio {portfolioId}.");

        await _accountRepository.DeleteAsync(account, ct);
        await _accountRepository.SaveChangesAsync(ct);
    }
}