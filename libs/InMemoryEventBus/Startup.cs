namespace PM.InMemoryEventBus;

using PM.SharedKernel.Events;
using Microsoft.Extensions.DependencyInjection;

public static class Setup
{
    public static async Task<IServiceProvider> StartConsumers(this IServiceProvider services)
    {
        var consumers = services.GetServices<IConsumer>();

        foreach (var consumer in consumers)
        {
            await consumer.Start().ConfigureAwait(false);
        }

        return services;
    }

    public static async Task<IServiceProvider> StartConsumers(this IServiceProvider services, CancellationToken parentToken)
    {
        var consumers = services.GetServices<IConsumer>();

        foreach (var consumer in consumers)
        {
            await consumer.Start(parentToken).ConfigureAwait(false);
        }

        return services;
    }

    public static async Task<IServiceProvider> StopConsumers(this IServiceProvider services)
    {
        var consumers = services.GetServices<IConsumer>();
        foreach (var consumer in consumers)
        {
            await consumer.Stop().ConfigureAwait(false);
        }

        return services;
    }

}