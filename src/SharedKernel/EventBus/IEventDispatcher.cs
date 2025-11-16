using PM.SharedKernel;

namespace PM.InMemoryEventBus;

public interface IDomainEventDispatcher
{
    public Task DispatchEntityEventsAsync(Entity entity, CancellationToken ct = default);
}