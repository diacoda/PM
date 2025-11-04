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
    public static PriceDTO ToDTO(AssetPrice price)
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
    public static AssetPrice ToEntity(PriceDTO dto)
    {
        var currency = Currency.CAD;
        var money = new Money(dto.Close, currency);
        Symbol s = new Symbol(dto.Symbol);
        return new AssetPrice(s, dto.Date, money, "Manual");
    }
}
