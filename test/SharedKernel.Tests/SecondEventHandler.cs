namespace PM.SharedKernel.Events.Tests;

public class SecondTestEventHandler : IDomainEventHandler<TestEvent>
{
    public bool WasCalled { get; private set; }

    public Task Handle(TestEvent domainEvent, CancellationToken ct)
    {
        WasCalled = true;
        return Task.CompletedTask;
    }
}
