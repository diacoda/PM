// from https://github.com/maranmaran/InMemChannelEventBus
// explained at: https://medium.com/@sociable_flamingo_goose_694/lightweight-net-channel-pub-sub-implementation-aed696337cc9

using PM.InMemoryEventBus.Contracts;

namespace PM.InMemoryEventBus.Implementation;

internal sealed class EventContextAccessor<T> : IEventContextAccessor<T>
{
    private static readonly AsyncLocal<EventMetadataWrapper<T>> Holder = new();

    public Event<T>? Event => Holder.Value?.Event;

    public void Set(Event<T> @event)
    {
        var holder = Holder.Value;
        if (holder != null)
        {
            holder.Event = null;
        }

        Holder.Value = new EventMetadataWrapper<T> { Event = @event };
    }
}

internal sealed class EventMetadataWrapper<T>
{
    public Event<T>? Event { get; set; }
}