namespace PM.SharedKernel;

public interface IDomainEventHandler<TEvent>
{
    Task Handle(TEvent @event, CancellationToken ct);
}
