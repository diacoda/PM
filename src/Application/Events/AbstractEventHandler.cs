namespace PM.Application.Events;

using PM.Domain.Events;
using PM.SharedKernel.Events;


/// <summary>
/// Base class for channel-based event handlers.
/// Automatically exposes Event<T> with metadata via the accessor.
/// </summary>
public abstract class AbstractEventHandler<TEvent> : IEventHandler<TEvent>
    where TEvent : IDomainEvent
{
    private readonly IEventContextAccessor<TEvent> _contextAccessor;

    protected AbstractEventHandler(IEventContextAccessor<TEvent> contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    /// <summary>
    /// Access the full event envelope (Event<T>) including metadata.
    /// </summary>
    protected Event<TEvent>? Context => _contextAccessor.Event;

    public virtual ValueTask Handle(TEvent? evt, CancellationToken ct = default)
    {
        // Default implementation: override in derived classes
        return ValueTask.CompletedTask;
    }
}
