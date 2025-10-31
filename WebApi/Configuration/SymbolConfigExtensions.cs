using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PM.Domain.Values;

namespace PM.API.Configuration;

public static class SymbolConfigExtensions
{
    public static IServiceCollection AddSymbolConfigs(this IServiceCollection services, IConfiguration config)
    {
        var symbolConfigs = config.GetSection("Symbols").Get<List<SymbolConfig>>() ?? new();
        var symbols = symbolConfigs.Select(s => new Symbol(s.Value, s.Currency, s.Exchange)).ToList();
        services.AddSingleton(symbols);
        return services;
    }
}
