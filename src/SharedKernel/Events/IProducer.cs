namespace PM.SharedKernel.Events;
/// <summary>
/// Publishes our custom event into event broker
/// </summary>
public interface IProducer<T> : IAsyncDisposable
{
    ValueTask Publish(Event<T> @event, CancellationToken token = default);
}