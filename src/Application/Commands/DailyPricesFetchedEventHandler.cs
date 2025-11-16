using PM.SharedKernel.Events;
using PM.Domain.Events;

namespace PM.Application.Commands;

public class DailyPricesFetchedEventHandler : IEventHandler<DailyPricesFetchedEvent>
{
    private readonly IEventContextAccessor<DailyPricesFetchedEvent> _ctx;

    public DailyPricesFetchedEventHandler(IEventContextAccessor<DailyPricesFetchedEvent> ctx)
        => _ctx = ctx;

    public ValueTask Handle(DailyPricesFetchedEvent evt, CancellationToken ct = default)
    {
        var correlationId = _ctx.Event?.Metadata?.CorrelationId ?? Guid.NewGuid().ToString();

        return ValueTask.CompletedTask;
    }
}
