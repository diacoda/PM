// from https://github.com/maranmaran/InMemChannelEventBus
// explained at: https://medium.com/@sociable_flamingo_goose_694/lightweight-net-channel-pub-sub-implementation-aed696337cc9

namespace PM.InMemoryEventBus.Contracts;

/// <summary>
/// Publishes our custom event into event broker
/// </summary>
public interface IProducer<T> : IAsyncDisposable
{
    ValueTask Publish(Event<T> @event, CancellationToken token = default);
}