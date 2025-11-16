namespace PM.InMemoryEventBus
{
    public record Event<T>(T Data, EventMetadata? Metadata = null);
    public record EventMetadata(string CorrelationId);

    public interface IEventHandler<in T>
    {
        ValueTask Handle(T data, CancellationToken ct = default);
    }

    public interface IEventBus
    {
        void Subscribe<T>(Func<IServiceProvider, IEventHandler<T>> factory);
        void Unsubscribe<T>(Func<IServiceProvider, IEventHandler<T>> factory);
        Task PublishAsync<T>(T evt, CancellationToken ct = default);
    }
}
