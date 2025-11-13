namespace PM.Application.Mappers;

using PM.Domain.Enums;
using PM.Domain.Values;
using PM.DTO.Prices;

public static class SymbolFetchDetailMapper
{
    public static SymbolFetchDetailDTO ToDto(SymbolFetchDetail domain)
    {
        return new SymbolFetchDetailDTO
        {
            Symbol = domain.Symbol,
            Exchange = domain.Exchange,
            Status = domain.Status,
            Error = domain.Error
        };
    }

    public static SymbolFetchDetail ToDomain(SymbolFetchDetailDTO dto)
    {
        return new SymbolFetchDetail(
            dto.Symbol,
            dto.Exchange,
            dto.Status,
            dto.Error
        );
    }

    public static List<SymbolFetchDetailDTO> ToDtoList(IEnumerable<SymbolFetchDetail> domainList)
    {
        return domainList.Select(ToDto).ToList();
    }

    public static List<SymbolFetchDetail> ToDomainList(IEnumerable<SymbolFetchDetailDTO> dtoList)
    {
        return dtoList.Select(ToDomain).ToList();
    }
}
