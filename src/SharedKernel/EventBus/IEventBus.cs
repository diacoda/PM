namespace PM.InMemoryEventBus
{
    
    



    public interface IEventBus
    {
        void Subscribe<T>(Func<IServiceProvider, IEventHandler<T>> factory);
        void Unsubscribe<T>(Func<IServiceProvider, IEventHandler<T>> factory);
        Task PublishAsync<T>(T evt, CancellationToken ct = default);
    }
}
