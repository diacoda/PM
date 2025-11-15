namespace PM.SharedKernel;

public interface IDomainEventDispatcher
{
    public Task DispatchEntityEventsAsync(Entity entity, CancellationToken ct = default);
}