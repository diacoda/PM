using PM.SharedKernel;

namespace PM.InMemoryEventBus;

public interface IEventDispatcher
{
    public Task DispatchEntityEventsAsync(Entity entity, CancellationToken ct = default);
}