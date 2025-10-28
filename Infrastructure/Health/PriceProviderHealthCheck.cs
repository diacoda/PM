namespace PM.Infrastructure.Health;

using PM.Domain.Values;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PM.Application.Interfaces;

public class PriceProviderHealthCheck : IHealthCheck
{
    private readonly IPriceProvider _priceProvider;

    public PriceProviderHealthCheck(IPriceProvider priceProvider)
    {
        _priceProvider = priceProvider ?? throw new ArgumentNullException(nameof(priceProvider));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // cheap probe; you could also pick a random symbol or check last refresh
            var price = await _priceProvider.GetPriceAsync(
                new Symbol("VFV.TO"),
                DateOnly.FromDateTime(DateTime.UtcNow)
            );

            if (price is null)
            {
                return HealthCheckResult.Unhealthy("Price provider returned null");
            }

            return HealthCheckResult.Healthy("Price provider OK");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Price provider failed", ex);
        }
    }
}
