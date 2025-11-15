using PM.Domain.Events;
using PM.SharedKernel.Events;
namespace PM.Application.Commands;

public class DailyPricesFetchedEventHandler
    : IDomainEventHandler<DailyPricesFetchedEvent>
{
    public Task Handle(DailyPricesFetchedEvent evt, CancellationToken ct)
    {
        Console.WriteLine($"[EVENT] DailyPricesFetchedEventHandler");
        return Task.CompletedTask;
    }
}