using Microsoft.Extensions.DependencyInjection;
namespace PM.SharedKernel.Events;

public class InMemoryDomainEventPublisher : IDomainEventPublisher
{
    private readonly IServiceScopeFactory _scopeFactory;

    // EventType → list of handler factories
    private readonly Dictionary<Type, List<Func<IServiceProvider, object>>> _registrations = new();

    public InMemoryDomainEventPublisher(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void Register<TEvent, THandler>()
        where TEvent : IDomainEvent
        where THandler : IDomainEventHandler<TEvent>
    {
        var evt = typeof(TEvent);

        if (!_registrations.TryGetValue(evt, out var list))
        {
            list = new List<Func<IServiceProvider, object>>();
            _registrations[evt] = list;
        }

        // DI factory
        list.Add(sp => sp.GetRequiredService<THandler>());
    }

    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : IDomainEvent
    {
        if (!_registrations.TryGetValue(typeof(TEvent), out var factories))
            return;

        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        foreach (var factory in factories)
        {
            IDomainEventHandler<TEvent> handler = null;
            try
            {
                handler = (IDomainEventHandler<TEvent>)factory(sp);
                await handler.Handle(domainEvent, ct);
            }
            catch (Exception ex)
            {
                // log but continue
                Console.WriteLine($"Handler {handler?.GetType().Name} failed: {ex}");
            }
        }
    }

}
