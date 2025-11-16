namespace PM.InMemoryEventBus;

public class EventHandlerWrapper<T> : IEventHandler<Event<T>>
{
    private readonly IEventHandler<T> _inner;

    public EventHandlerWrapper(IEventHandler<T> inner) => _inner = inner;

    public ValueTask Handle(Event<T> evt, CancellationToken ct = default)
    {
        return _inner.Handle(evt.Data!, ct); // unwrap Event<T> to pass to your real handler
    }
}
