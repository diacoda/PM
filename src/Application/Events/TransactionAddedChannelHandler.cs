using PM.SharedKernel.Events;
using PM.Domain.Events;
using PM.Application.Events;
using Microsoft.Extensions.Logging;

namespace PM.Application.Events;

public class TransactionAddedChannelHandler
    : AbstractEventHandler<TransactionAddedEvent>
{
    private readonly ILogger<TransactionAddedChannelHandler> _logger;

    public TransactionAddedChannelHandler(
        IEventContextAccessor<TransactionAddedEvent> accessor,
        ILogger<TransactionAddedChannelHandler> logger)
        : base(accessor)
    {
        _logger = logger;
    }

    public override ValueTask Handle(TransactionAddedEvent? evt, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Handled TxAdded: {TxId}, Meta: {CorrelationId}",
            evt?.TransactionId,
            Context?.Metadata?.CorrelationId
        );

        return ValueTask.CompletedTask;
    }
}
