using PM.Domain.Events;
using PM.InMemoryEventBus;

namespace PM.Application.Commands;

/*public class TransactionAddedHandler : IEventHandler<TransactionAddedEvent>
{
    public ValueTask Handle(TransactionAddedEvent evt, CancellationToken ct = default)
    {
        Console.WriteLine($"[EVENT] Transaction {evt.TransactionId} added to Account {evt.AccountId}, Portfolio {evt.PortfolioId} on {evt.Date}");
        return ValueTask.CompletedTask;
    }
}*/

public class TransactionAddedHandler : IEventHandler<TransactionAddedEvent>
{
    public ValueTask Handle(TransactionAddedEvent evt, CancellationToken ct = default)
    {
        Console.WriteLine($"[HANDLER] Transaction {evt.TransactionId} added to Account {evt.AccountId}, Portfolio {evt.PortfolioId} on {evt.Date}");
        return ValueTask.CompletedTask;
    }
}
