
using PM.Domain.Values;
using PM.DTO.Prices;

namespace PM.Application.Interfaces;

public interface IPriceService
{
    Task<AssetPrice?> GetOrFetchInstrumentPriceAsync(string symbolValue, DateOnly date, CancellationToken ct = default);
    Task<PriceDTO> UpdatePriceAsync(string symbolValue, UpdatePriceRequest request, CancellationToken ct);
    Task<PriceDTO?> GetPriceAsync(string symbolValue, DateOnly date, CancellationToken ct);
    Task<List<PriceDTO>> GetAllPricesForSymbolAsync(string symbolValue, CancellationToken ct);
    Task<bool> DeletePriceAsync(string symbolValue, DateOnly date, CancellationToken ct);
    Task<List<PriceDTO>> GetAllPricesByDateAsync(DateOnly date, CancellationToken ct);
}