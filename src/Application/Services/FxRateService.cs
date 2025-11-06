using PM.Application.Interfaces;
using PM.Domain.Values;

namespace PM.Application.Services;

public class FxRateService : IFxRateService
{
    private readonly IFxRateRepository _repository;
    private readonly List<Currency> _currencies = new List<Currency>();

    public FxRateService(IFxRateRepository repository)
    {
        _repository = repository;
        _currencies.Add(new Currency("CAD"));
        _currencies.Add(new Currency("USD"));
        _currencies.Add(new Currency("EUR"));
    }

    public async Task<FxRate> UpdateRateAsync(
        string fromCurrencyCode,
        string toCurrencyCode,
        decimal rate,
        DateOnly date,
        CancellationToken ct = default)
    {
        var from = _currencies.FirstOrDefault(c => c.Code == fromCurrencyCode)
                   ?? throw new ArgumentException($"Unknown currency '{fromCurrencyCode}'");
        var to = _currencies.FirstOrDefault(c => c.Code == toCurrencyCode)
                 ?? throw new ArgumentException($"Unknown currency '{toCurrencyCode}'");

        var fxRate = new FxRate(from, to, date, rate);
        await _repository.UpsertAsync(fxRate, ct);
        return fxRate;
    }

    public async Task<FxRate?> GetRateAsync(string fromCurrencyCode, string toCurrencyCode, DateOnly date, CancellationToken ct = default)
    {
        var from = _currencies.FirstOrDefault(c => c.Code == fromCurrencyCode)
                   ?? throw new ArgumentException($"Unknown currency '{fromCurrencyCode}'");
        var to = _currencies.FirstOrDefault(c => c.Code == toCurrencyCode)
                 ?? throw new ArgumentException($"Unknown currency '{toCurrencyCode}'");

        return await _repository.GetAsync(from, to, date, ct);
    }

    public async Task<List<FxRate>> GetAllRatesForPairAsync(string fromCurrencyCode, string toCurrencyCode, CancellationToken ct = default)
    {
        var from = _currencies.FirstOrDefault(c => c.Code == fromCurrencyCode)
                   ?? throw new ArgumentException($"Unknown currency '{fromCurrencyCode}'");
        var to = _currencies.FirstOrDefault(c => c.Code == toCurrencyCode)
                 ?? throw new ArgumentException($"Unknown currency '{toCurrencyCode}'");

        return await _repository.GetAllForPairAsync(from, to, ct);
    }

    public async Task<bool> DeleteRateAsync(string fromCurrencyCode, string toCurrencyCode, DateOnly date, CancellationToken ct = default)
    {
        var from = _currencies.FirstOrDefault(c => c.Code == fromCurrencyCode)
                   ?? throw new ArgumentException($"Unknown currency '{fromCurrencyCode}'");
        var to = _currencies.FirstOrDefault(c => c.Code == toCurrencyCode)
                 ?? throw new ArgumentException($"Unknown currency '{toCurrencyCode}'");

        return await _repository.DeleteAsync(from, to, date, ct);
    }

    public async Task<List<FxRate>> GetAllRatesByDateAsync(DateOnly date, CancellationToken ct = default)
    {
        return await _repository.GetAllByDateAsync(date, ct);
    }
}
