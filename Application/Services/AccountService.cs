using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.Application.Interfaces;

namespace model.Application.Services
{
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

        public async Task<Account> CreateAsync(
            string name,
            Currency currency,
            FinancialInstitutions financialInstitution,
            CancellationToken ct = default)
        {
            var account = new Account(name, currency, financialInstitution);
            await _accountRepository.AddAsync(account, ct);
            await _accountRepository.SaveChangesAsync(ct);
            return account;
        }

        public async Task AddAccountToPortfolioAsync(int portfolioId, int accountId, CancellationToken ct = default)
        {
            var portfolio = await _portfolioRepository.GetByIdAsync(portfolioId, ct)
                ?? throw new KeyNotFoundException($"Portfolio with ID {portfolioId} not found.");

            var account = await _accountRepository.GetByIdAsync(accountId, ct)
                ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");

            portfolio.AddAccount(account);
            await _portfolioRepository.SaveChangesAsync(ct);
        }

        public async Task RemoveAccountFromPortfolioAsync(int portfolioId, int accountId, CancellationToken ct = default)
        {
            var portfolio = await _portfolioRepository.GetByIdAsync(portfolioId, ct)
                ?? throw new KeyNotFoundException($"Portfolio with ID {portfolioId} not found.");

            var account = portfolio.Accounts.FirstOrDefault(a => a.Id == accountId)
                ?? throw new KeyNotFoundException($"Account {accountId} not found in Portfolio {portfolioId}.");

            portfolio.RemoveAccount(account);
            await _portfolioRepository.SaveChangesAsync(ct);
        }

        public async Task<Account?> GetAccountAsync(int portfolioId, int accountId, CancellationToken ct = default)
        {
            var portfolio = await _portfolioRepository.GetByIdAsync(portfolioId, ct);
            return portfolio?.Accounts.FirstOrDefault(a => a.Id == accountId);
        }

        public async Task<IEnumerable<Account>> ListAccountsAsync(int portfolioId, CancellationToken ct = default)
        {
            var portfolio = await _portfolioRepository.GetByIdAsync(portfolioId, ct)
                ?? throw new KeyNotFoundException($"Portfolio with ID {portfolioId} not found.");

            return portfolio.Accounts;
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
            await _accountRepository.SaveChangesAsync();
        }

        public IEnumerable<Holding> GetHoldingsByTag(Account account, Tag tag)
        {
            return account.Holdings
                .Where(h => h.Tags.Contains(tag))
                .ToList();
        }
    }
}
