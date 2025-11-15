namespace PM.SharedKernel.Events;

public interface IDomainEventDispatcher
{
    public Task DispatchEntityEventsAsync(Entity entity, CancellationToken ct = default);
}