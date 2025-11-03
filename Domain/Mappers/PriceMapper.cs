using PM.Domain.Values;
using PM.DTO.Prices;

namespace PM.Domain.Mappers;

/// <summary>
/// Provides mapping between domain entities and DTOs for prices.
/// </summary>
public static class PriceMapper
{
    /// <summary>
    /// Converts a domain <see cref="InstrumentPrice"/> to a <see cref="PriceDTO"/>.
    /// </summary>
    public static PriceDTO ToDTO(InstrumentPrice price)
    {
        return new PriceDTO
        {
            Symbol = price.Symbol.Code,
            Date = price.Date,
            Close = price.Price.Amount
        };
    }

    /// <summary>
    /// Converts a <see cref="PriceDTO"/> to a domain <see cref="InstrumentPrice"/>.
    /// </summary>
    /// <param name="dto">The price DTO.</param>
    /// <param name="symbol">The symbol entity corresponding to the DTO's symbol value.</param>
    /// <param name="currency">The currency of the price.</param>
    /// <param name="source">The source of the price (e.g., "Manual Entry", "Yahoo").</param>
    public static InstrumentPrice ToEntity(PriceDTO dto)
    {
        Currency cad = new Currency("CAD");
        var money = new Money(dto.Close, cad);
        Symbol s = new Symbol(dto.Symbol, "CAD");
        return new InstrumentPrice(s, dto.Date, money, cad, "Manual");
    }
}
