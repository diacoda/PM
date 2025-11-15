namespace PM.SharedKernel;

public interface IDomainEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class;

    void Subscribe<TEvent, THandler>()
        where TEvent : class
        where THandler : IDomainEventHandler<TEvent>;
}