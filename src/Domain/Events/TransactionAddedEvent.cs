using PM.SharedKernel;

namespace PM.Domain.Events;

public record TransactionAddedEvent(int PortfolioId, int AccountId, int TransactionId, DateOnly date) : IDomainEvent;
