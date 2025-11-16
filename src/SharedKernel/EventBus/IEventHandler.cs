namespace PM.InMemoryEventBus;

public interface IEventHandler<T>
{
    ValueTask Handle(T data, CancellationToken ct = default);
}