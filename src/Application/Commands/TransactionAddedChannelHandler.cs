using PM.SharedKernel.Events;
using PM.Domain.Events;

namespace PM.Application.Commands;

// Implements IEventHandler<TransactionAddedEvent> directly
public class TransactionAddedChannelHandler : IEventHandler<TransactionAddedEvent>
{
    private readonly IEventContextAccessor<TransactionAddedEvent> _ctx;

    public TransactionAddedChannelHandler(IEventContextAccessor<TransactionAddedEvent> ctx)
        => _ctx = ctx;

    public ValueTask Handle(TransactionAddedEvent? evt, CancellationToken ct = default)
    {
        var correlationId = _ctx.Event?.Metadata?.CorrelationId ?? Guid.NewGuid().ToString();
        Console.WriteLine($"[HANDLER] Transaction {evt?.TransactionId} added. CorrelationId={correlationId}");
        return ValueTask.CompletedTask;
    }
}
