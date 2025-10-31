using PM.API.HostedServices;
using PM.Application.Commands;
using PM.Application.Services;
using PM.Application.Interfaces;

namespace PM.API.Configuration;

public static class HostedServiceConfig
{
    public static IServiceCollection AddHostedJobs(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<PriceJobOptions>(config.GetSection("PriceJobOptions"));
        services.Configure<MarketHolidaysConfig>(config.GetSection("MarketHolidays"));

        var holidays = new MarketHolidaysConfig();
        config.GetSection("MarketHolidays").Bind(holidays);
        services.AddSingleton<IMarketCalendar>(new MarketCalendar(holidays));

        services.AddHostedService<DailyPriceService>();
        services.AddScoped<FetchDailyPricesCommand>();

        return services;
    }
}
