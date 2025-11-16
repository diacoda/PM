using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
namespace PM.InMemoryEventBus;

public class InMemoryEventBus : IEventBus
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentDictionary<Type, List<Func<IServiceProvider, object>>> _registrations
        = new();

    public InMemoryEventBus(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void Subscribe<T>(Func<IServiceProvider, IEventHandler<T>> factory)
    {
        var list = _registrations.GetOrAdd(typeof(T), _ => new List<Func<IServiceProvider, object>>());
        lock (list)
        {
            list.Add(sp => factory(sp));
        }
    }

    public void Unsubscribe<T>(Func<IServiceProvider, IEventHandler<T>> factory)
    {
        if (_registrations.TryGetValue(typeof(T), out var list))
        {
            lock (list)
            {
                list.RemoveAll(f => f.Target == factory.Target && f.Method == factory.Method);
            }
        }
    }

    public async Task PublishAsync<T>(T evt, CancellationToken ct = default)
    {
        if (!_registrations.TryGetValue(typeof(T), out var factories)) return;

        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        List<IEventHandler<T>> handlers;
        lock (factories)
        {
            handlers = factories.Select(f => (IEventHandler<T>)f(sp)).ToList();
        }

        foreach (var h in handlers)
        {
            try
            {
                await h.Handle(evt, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handler {h.GetType().Name} failed: {ex}");
            }
        }
    }
}
