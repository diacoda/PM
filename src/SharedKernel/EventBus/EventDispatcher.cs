using PM.SharedKernel;

namespace PM.InMemoryEventBus;

public class EventDispatcher : IDomainEventDispatcher
{
    private readonly IEventBus _bus;
    public EventDispatcher(IEventBus bus) => _bus = bus;

    public async Task DispatchEntityEventsAsync(Entity entity, CancellationToken ct = default)
    {
        var events = entity.DomainEvents.ToList();
        entity.ClearDomainEvents();

        foreach (var domainEvent in events)
        {
            await DispatchAsync(domainEvent, ct);
        }
    }

    private async Task DispatchAsync<T>(T domainEvent, CancellationToken ct = default)
    {
        if (domainEvent is null) return;
        await _bus.PublishAsync((dynamic)domainEvent, ct);
        /*

                if (domainEvent is null) return;

            var metadata = new EventMetadata(Guid.NewGuid().ToString()); // correlation ID
            var evt = new Event<T>(domainEvent, metadata);

            await _bus.PublishAsync(evt, ct);
            */
    }
}
