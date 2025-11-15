namespace PM.SharedKernel.Events.Tests;

public class TestEntity : Entity
{
    public void AddEvent(IDomainEvent evt)
    {
        Raise(evt);
    }
}
