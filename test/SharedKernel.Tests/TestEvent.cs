namespace PM.SharedKernel.Events.Tests;

public record TestEvent(string Message) : IDomainEvent;
