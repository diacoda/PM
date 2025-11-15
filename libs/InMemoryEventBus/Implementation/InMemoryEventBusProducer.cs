// from https://github.com/maranmaran/InMemChannelEventBus
// explained at: https://medium.com/@sociable_flamingo_goose_694/lightweight-net-channel-pub-sub-implementation-aed696337cc9

using System.Threading.Channels;
using PM.InMemoryEventBus.Contracts;

namespace PM.InMemoryEventBus.Implementation;

internal sealed class InMemoryEventBusProducer<T> : IProducer<T>
{
    private readonly ChannelWriter<Event<T>> _bus;

    public InMemoryEventBusProducer(ChannelWriter<Event<T>> bus)
    {
        _bus = bus;
    }

    public async ValueTask Publish(Event<T> @event, CancellationToken token = default)
    {
        await _bus.WriteAsync(@event, token).ConfigureAwait(false);
    }

    public ValueTask DisposeAsync()
    {
        _bus.TryComplete();

        return ValueTask.CompletedTask;
    }
}