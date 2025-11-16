using PM.SharedKernel.Events;
using PM.Domain.Events;

namespace PM.Application.Events;

public class SendNotificationOnTransactionAdded : AbstractEventHandler<TransactionAddedEvent>
{


    public SendNotificationOnTransactionAdded(IEventContextAccessor<TransactionAddedEvent> ctx)
        : base(ctx)
    {

    }

    public override ValueTask Handle(TransactionAddedEvent? evt, CancellationToken ct = default)
    {
        var correlationId = Context?.Metadata?.CorrelationId ?? Guid.NewGuid().ToString();
        Console.WriteLine($"[HANDLER] Transaction {evt?.TransactionId} added. CorrelationId={correlationId}");
        return ValueTask.CompletedTask;
    }
}
