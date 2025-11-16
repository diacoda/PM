using Microsoft.Extensions.Hosting;
using PM.SharedKernel.Events;
using Microsoft.Extensions.DependencyInjection;

namespace PM.Infrastructure.EventBus;

public class EventBusHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<IConsumer> _consumers = new();

    public EventBusHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var consumers = scope.ServiceProvider.GetServices<IConsumer>();
        _consumers.AddRange(consumers);

        // Start all consumers
        foreach (var consumer in _consumers)
        {
            await consumer.Start(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Stop all consumers
        foreach (var consumer in _consumers)
        {
            await consumer.Stop(cancellationToken);
        }
    }
}
