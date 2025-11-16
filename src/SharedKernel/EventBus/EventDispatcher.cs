using PM.SharedKernel;

namespace PM.InMemoryEventBus;

public class EventDispatcher : IEventDispatcher
{
    private readonly IEventBus _bus;
    public EventDispatcher(IEventBus bus) => _bus = bus;

    public async Task DispatchEntityEventsAsync(Entity entity, CancellationToken ct = default)
    {
        var events = entity.DomainEvents.ToList();
        entity.ClearDomainEvents();

        foreach (var domainEvent in events)
        {
            var evtType = typeof(Event<>).MakeGenericType(domainEvent.GetType());
            var evtInstance = Activator.CreateInstance(evtType, domainEvent, new EventMetadata(Guid.NewGuid().ToString()));
            await _bus.PublishAsync((dynamic)evtInstance, ct); // type-safe dynamic dispatch
        }
    }
}