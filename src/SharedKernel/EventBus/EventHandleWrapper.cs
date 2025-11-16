namespace PM.InMemoryEventBus;

public class EventHandlerWrapper<T> : IEventHandler<Event<T>>
{
    private readonly IEventHandler<T> _inner;

    public EventHandlerWrapper(IEventHandler<T> inner) => _inner = inner;

    public ValueTask Handle(Event<T> evt, CancellationToken ct = default)
    {
        Console.WriteLine(evt.Metadata?.CorrelationId);
        return _inner.Handle(evt.Data, ct); // unwrap Event<T>
    }
}
