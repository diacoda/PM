namespace PM.SharedKernel.Events.Tests;

public class TestEventHandler : IDomainEventHandler<TestEvent>
{
    public bool WasCalled { get; private set; }
    public TestEvent? ReceivedEvent { get; private set; }

    public Task Handle(TestEvent domainEvent, CancellationToken ct)
    {
        WasCalled = true;
        ReceivedEvent = domainEvent;
        return Task.CompletedTask;
    }
}
