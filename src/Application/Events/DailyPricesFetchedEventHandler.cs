using PM.SharedKernel.Events;
using PM.Domain.Events;

namespace PM.Application.Events;

public class DailyPricesFetchedEventHandler
    : AbstractEventHandler<DailyPricesFetchedEvent>
{
    public DailyPricesFetchedEventHandler(IEventContextAccessor<DailyPricesFetchedEvent> accesor)
        : base(accesor)
    {
    }

    public override ValueTask Handle(DailyPricesFetchedEvent? evt, CancellationToken ct = default)
    {
        var correlationId = Context?.Metadata?.CorrelationId ?? Guid.NewGuid().ToString();
        if (evt!.AllSucceeded)
        {
            //
        }
        return ValueTask.CompletedTask;
    }
}
