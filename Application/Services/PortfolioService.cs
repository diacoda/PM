using PM.Application.Interfaces;
using PM.Domain.Entities;

namespace PM.Application.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioRepository _repo;

        public PortfolioService(IPortfolioRepository repo)
        {
            _repo = repo;
        }

        public async Task<Portfolio> CreateAsync(string owner, CancellationToken ct = default)
        {
            var portfolio = new Portfolio(owner);
            await _repo.AddAsync(portfolio, ct);
            await _repo.SaveChangesAsync(ct);
            return portfolio;
        }

        public async Task<Portfolio?> GetByIdAsync(int portfolioId, CancellationToken ct = default) =>
            await _repo.GetByIdAsync(portfolioId, ct);

        public async Task<IEnumerable<Portfolio>> ListAsync(CancellationToken ct = default) =>
            await _repo.ListAsync(ct);

        public async Task UpdateOwnerAsync(int portfolioId, string newOwner, CancellationToken ct = default)
        {
            var portfolio = await _repo.GetByIdAsync(portfolioId, ct);
            if (portfolio == null) return;
            portfolio.Owner = newOwner;
            await _repo.UpdateAsync(portfolio, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int portfolioId, CancellationToken ct = default)
        {
            var portfolio = await _repo.GetByIdAsync(portfolioId, ct)
                ?? throw new KeyNotFoundException($"Portfolio with ID {portfolioId} not found.");

            await _repo.DeleteAsync(portfolio, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<Account>> GetAccountsByTagAsync(int portfolioId, Tag tag, CancellationToken ct = default)
        {
            var portfolio = await _repo.GetByIdAsync(portfolioId, ct);
            if (portfolio == null) return Enumerable.Empty<Account>();

            return portfolio.Accounts.Where(a => a.Tags.Contains(tag)).ToList();
        }
    }
}
