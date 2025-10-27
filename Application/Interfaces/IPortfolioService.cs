using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IPortfolioService
{
    Task<Portfolio> CreateAsync(string owner, CancellationToken ct = default);

    Task<Portfolio?> GetById(int portfolioId, CancellationToken ct = default);

    Task<IEnumerable<Portfolio>> ListAsync(CancellationToken ct = default);

    Task UpdateOwnerAsync(int portfolioId, string newOwner, CancellationToken ct = default);
    Task DeleteAsync(int portfolioId, CancellationToken ct = default);
    Task<IEnumerable<Account>> GetAccountsByTagAsync(int portfolioId, Tag tag, CancellationToken ct = default);
}