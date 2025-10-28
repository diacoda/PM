using PM.DTO;
using PM.Domain.Entities;
using PM.Domain.Values;

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
}
