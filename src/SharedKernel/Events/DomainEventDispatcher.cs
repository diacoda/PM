namespace PM.SharedKernel.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IDomainEventPublisher _publisher;

    public DomainEventDispatcher(IDomainEventPublisher publisher)
        => _publisher = publisher;

    public async Task DispatchEntityEventsAsync(Entity entity, CancellationToken ct = default)
    {
        var events = entity.DomainEvents.ToList();
        entity.ClearDomainEvents();

        foreach (var domainEvent in events)
            await _publisher.PublishAsync((dynamic)domainEvent, ct);
    }
}