using PM.Domain.Entities;
using PM.Domain.Values;
using PM.DTO;
using PM.SharedKernel;

namespace PM.Application.Interfaces;

public interface IPortfolioService
{
    Task<PortfolioDTO> CreateAsync(string owner, CancellationToken ct = default);
    Task UpdateOwnerAsync(int portfolioId, string newOwner, CancellationToken ct = default);
    Task DeleteAsync(int portfolioId, CancellationToken ct = default);
    Task<IEnumerable<AccountDTO>> GetAccountsByTagAsync(int portfolioId, Tag tag, CancellationToken ct = default);
    Task<PortfolioDTO?> GetByIdAsync(int portfolioId, CancellationToken ct = default);
    Task<PortfolioDTO?> GetByIdWithIncludesAsync(int portfolioId, IncludeOption[] includes, CancellationToken ct = default);
    Task<IEnumerable<PortfolioDTO>> ListAsync(CancellationToken ct = default);
    Task<IEnumerable<PortfolioDTO>> ListWithIncludesAsync(IncludeOption[] includes, CancellationToken ct = default);

}