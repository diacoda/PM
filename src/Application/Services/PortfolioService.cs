using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.DTO;
using PM.Domain.Mappers;
using PM.SharedKernel;

namespace PM.Application.Services;

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _portfolioRepo;
    private readonly IAccountRepository _accountRepo;

    public PortfolioService(IPortfolioRepository portfolioRepo, IAccountRepository accountRepo)
    {
        _portfolioRepo = portfolioRepo;
        _accountRepo = accountRepo;
    }

    // 1. Create portfolio
    public async Task<PortfolioDTO> CreateAsync(string owner, CancellationToken ct = default)
    {
        var portfolio = new Portfolio(owner);
        await _portfolioRepo.AddAsync(portfolio, ct);
        await _portfolioRepo.SaveChangesAsync(ct);
        return PortfolioMapper.ToDTO(portfolio);
    }

    // 2. Update portfolio owner
    public async Task UpdateOwnerAsync(int portfolioId, string newOwner, CancellationToken ct = default)
    {
        var portfolio = await _portfolioRepo.GetByIdAsync(portfolioId, ct);
        if (portfolio == null) return;

        portfolio.Owner = newOwner;
        await _portfolioRepo.UpdateAsync(portfolio, ct);
        await _portfolioRepo.SaveChangesAsync(ct);
    }

    // 3. Delete portfolio
    public async Task DeleteAsync(int portfolioId, CancellationToken ct = default)
    {
        var portfolio = await _portfolioRepo.GetByIdAsync(portfolioId, ct)
            ?? throw new KeyNotFoundException($"Portfolio with ID {portfolioId} not found.");

        await _portfolioRepo.DeleteAsync(portfolio, ct);
        await _portfolioRepo.SaveChangesAsync(ct);
    }

    public async Task<PortfolioDTO?> GetByIdAsync(int portfolioId, CancellationToken ct = default)
    {
        var portfolio = await _portfolioRepo.GetByIdAsync(portfolioId, ct);
        return portfolio == null ? null : PortfolioMapper.ToDTO(portfolio);
    }

    public async Task<PortfolioDTO?> GetByIdWithIncludesAsync(int portfolioId, IncludeOption[] includes, CancellationToken ct = default)
    {
        var portfolio = await _portfolioRepo.GetByIdWithIncludesAsync(portfolioId, includes, ct);
        return portfolio == null ? null : PortfolioMapper.ToDTO(portfolio, includes);
    }

    public async Task<IEnumerable<PortfolioDTO>> ListAsync(CancellationToken ct = default)
    {
        var portfolios = await _portfolioRepo.ListAsync(ct);
        return portfolios.Select(PortfolioMapper.ToDTO).ToList();
    }

    // 7. List all portfolios with includes
    public async Task<IEnumerable<PortfolioDTO>> ListWithIncludesAsync(IncludeOption[] includes, CancellationToken ct = default)
    {
        var portfolios = await _portfolioRepo.ListWithIncludesAsync(includes, ct);
        return portfolios.Select(p => PortfolioMapper.ToDTO(p, includes)).ToList();
    }

    // 8. Get accounts by tag
    public async Task<IEnumerable<AccountDTO>> GetAccountsByTagAsync(int portfolioId, Tag tag, CancellationToken ct = default)
    {
        var portfolio = await _portfolioRepo.GetByIdAsync(portfolioId, ct);
        if (portfolio == null) return Enumerable.Empty<AccountDTO>();

        var taggedAccounts = portfolio.Accounts
            .Where(a => a.Tags.Contains(tag))
            .Select(AccountMapper.ToDTO)
            .ToList();

        return taggedAccounts;
    }
}
