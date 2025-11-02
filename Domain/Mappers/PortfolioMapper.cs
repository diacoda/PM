using PM.DTO;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.SharedKernel;

namespace PM.Domain.Mappers;

public static class PortfolioMapper
{
    public static Portfolio ToEntity(PortfolioDTO dto) =>
        new Portfolio(dto.Owner);

    /// <summary>
    /// Maps a PortfolioDTO to a Portfolio entity, optionally including Accounts.
    /// </summary>
    public static Portfolio ToEntity(PortfolioDTO dto, IncludeOption[] includes)
    {
        var portfolio = new Portfolio(dto.Owner);

        if (includes.Contains(IncludeOption.Accounts) && dto.Accounts is not null)
        {
            foreach (var accountDto in dto.Accounts)
            {
                var account = AccountMapper.ToEntity(accountDto, includes);
                portfolio.AddAccount(account);
            }
        }

        return portfolio;
    }
    public static PortfolioDTO ToDTO(Portfolio entity) =>
        new PortfolioDTO()
        {
            Owner = entity.Owner,
            Id = entity.Id
        };

    public static PortfolioDTO ToDTO(Portfolio portfolio, IncludeOption[] includes)
    {
        var dto = new PortfolioDTO
        {
            Id = portfolio.Id,
            Owner = portfolio.Owner,
            Accounts = includes.Contains(IncludeOption.Accounts)
                ? portfolio.Accounts.Select(a => AccountMapper.ToDTO(a, includes)).ToList()
                : new List<AccountDTO>()
        };

        return dto;
    }
}
