namespace PM.SharedKernel.Events;
/// <summary>
/// Event metadata accessor in current async context
/// </summary>
public interface IEventContextAccessor<T>
{
    public Event<T>? Event { get; }

    void Set(Event<T> @event);
}