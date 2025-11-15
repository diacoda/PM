// from https://github.com/maranmaran/InMemChannelEventBus
// explained at: https://medium.com/@sociable_flamingo_goose_694/lightweight-net-channel-pub-sub-implementation-aed696337cc9

namespace PM.InMemoryEventBus.Contracts;
/// <summary>
/// Starts processing our bus
/// We can manipulate Subscribe and Unsubscribe methods to
/// turn processing on or off
/// </summary>
public interface IConsumer : IAsyncDisposable
{
    ValueTask Start(CancellationToken token = default);

    ValueTask Stop(CancellationToken token = default);
}

public interface IConsumer<T> : IConsumer
{
}