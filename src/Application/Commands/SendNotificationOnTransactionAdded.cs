using PM.SharedKernel;
using PM.Domain.Events;

public class SendNotificationOnTransactionAdded : IDomainEventHandler<TransactionAddedEvent>
{
    public Task Handle(TransactionAddedEvent evt, CancellationToken ct)
    {
        Console.WriteLine($"[EVENT] Transaction {evt.TransactionId} added to Account {evt.AccountId}, Portfolio {evt.PortfolioId} on {evt.Date}");
        return Task.CompletedTask;
    }
}