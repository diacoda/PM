// from https://github.com/maranmaran/InMemChannelEventBus
// explained at: https://medium.com/@sociable_flamingo_goose_694/lightweight-net-channel-pub-sub-implementation-aed696337cc9
namespace PM.InMemoryEventBus.Contracts;

/// <summary>
/// Event metadata accessor in current async context
/// </summary>
public interface IEventContextAccessor<T>
{
    public Event<T>? Event { get; }

    void Set(Event<T> @event);
}