using PM.Domain.Entities;
using PM.Domain.Values;
using PM.DTO;

namespace PM.Domain.Mappers;
public static class HoldingMapper
{
    public static HoldingDTO ToDTO(Holding holding) => new HoldingDTO
    {
        Id = holding.Id,
        Symbol = holding.Symbol.Value,
        Quantity = holding.Quantity,
        AccountId = holding.AccountId,
        Tags = holding.Tags.Select(t => t.Name).ToList()
    };

    public static Holding ToEntity(HoldingDTO dto, Symbol symbol) =>
        new Holding(symbol, dto.Quantity)
        {
            AccountId = dto.AccountId
        };
}