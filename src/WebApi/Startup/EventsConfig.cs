using PM.Application.Events;
using PM.Domain.Events;
using PM.Infrastructure.EventBus;
using PM.Infrastructure.EventBus.Registration;

namespace PM.API.Startup;

/// <summary>
/// EventBus configuration
/// </summary>
public static class EventsConfig
{
    /// <summary>
    /// Extension method to add event bus configuration
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddEventBusConfig(this IServiceCollection services)
    {
        services.AddHostedService<EventBusHostedService>();
        services.AddInMemoryEvent<TransactionAddedEvent, TransactionAddedChannelHandler>();
        services.AddInMemoryEvent<TransactionAddedEvent, SendNotificationOnTransactionAdded>();
        services.AddInMemoryEvent<DailyPricesFetchedEvent, DailyPricesFetchedEventHandler>();

        return services;
    }
}