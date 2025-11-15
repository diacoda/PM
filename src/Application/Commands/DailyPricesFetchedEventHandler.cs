using PM.SharedKernel;
using PM.Domain.Events;
namespace PM.Application.Commands;

public class DailyPricesFetchedEventHandler
    : IDomainEventHandler<DailyPricesFetchedEvent>
{
    public Task Handle(DailyPricesFetchedEvent @event, CancellationToken ct)
    {
        Console.WriteLine($"[EVENT] DailyPricesFetchedEventHandler");
        return Task.CompletedTask;
    }
}