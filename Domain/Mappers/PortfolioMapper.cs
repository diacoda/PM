using PM.DTO;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.SharedKernel;

namespace PM.Domain.Mappers;

public static class PortfolioMapper
{
    public static Portfolio ToEntity(PortfolioDTO dto) =>
        new Portfolio(dto.Owner);

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
