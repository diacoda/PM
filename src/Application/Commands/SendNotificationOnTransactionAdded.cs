using PM.Domain.Events;
using PM.SharedKernel.Events;
namespace PM.Application.Commands;

public class SendNotificationOnTransactionAdded
    : IDomainEventHandler<TransactionAddedEvent>
{
    public Task Handle(TransactionAddedEvent evt, CancellationToken ct)
    {
        Console.WriteLine($"[EVENT] Transaction {evt.TransactionId} added to Account {evt.AccountId}, Portfolio {evt.PortfolioId} on {evt.Date}");
        return Task.CompletedTask;
    }
}