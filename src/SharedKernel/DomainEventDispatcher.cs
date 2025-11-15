namespace PM.SharedKernel;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IDomainEventPublisher _publisher;

    public DomainEventDispatcher(IDomainEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task DispatchEntityEventsAsync(Entity entity, CancellationToken ct = default)
    {
        foreach (var evt in entity.DomainEvents)
            await _publisher.PublishAsync((dynamic)evt, ct);

        entity.ClearDomainEvents();
    }
}
