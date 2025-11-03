using PM.Domain.Entities;
using PM.Domain.Values;
using PM.DTO;

namespace PM.Domain.Mappers;

public static class HoldingMapper
{
    public static HoldingDTO ToDTO(Holding holding) => new HoldingDTO
    {
        Id = holding.Id,
        Symbol = holding.Symbol.Code,
        Quantity = holding.Quantity,
        AccountId = holding.AccountId,
        Tags = holding.Tags.Select(t => t.Name).ToList()
    };

    /// <summary>
    /// Converts a HoldingDTO to a Holding entity, constructing its Symbol and optionally its Tags.
    /// </summary>
    public static Holding ToEntity(HoldingDTO dto)
    {
        var symbol = new Symbol(dto.Symbol);

        var holding = new Holding(symbol, dto.Quantity)
        {
            AccountId = dto.AccountId
        };

        if (dto.Tags is not null && dto.Tags.Count > 0)
        {
            foreach (var tagName in dto.Tags)
            {
                holding.AddTag(new Tag(tagName));
            }
        }

        return holding;
    }

    /// <summary>
    /// Converts a HoldingDTO to a Holding entity using an existing Symbol instance (optional optimization).
    /// </summary>
    public static Holding ToEntity(HoldingDTO dto, Symbol symbol)
    {
        var holding = new Holding(symbol, dto.Quantity)
        {
            AccountId = dto.AccountId
        };

        if (dto.Tags is not null && dto.Tags.Count > 0)
        {
            foreach (var tagName in dto.Tags)
            {
                holding.AddTag(new Tag(tagName));
            }
        }

        return holding;
    }
}
