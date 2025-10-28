using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IPriceProvider
{
    Task<InstrumentPrice?> GetPriceAsync(Symbol symbol, DateOnly date);
    string ProviderName { get; }
}
