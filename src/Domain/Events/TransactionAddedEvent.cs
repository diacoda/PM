using PM.SharedKernel.Events;

namespace PM.Domain.Events;

public record TransactionAddedEvent(int PortfolioId, int AccountId, int TransactionId, DateOnly Date) : IDomainEvent;
