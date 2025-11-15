using Microsoft.Extensions.DependencyInjection;

namespace PM.SharedKernel;

public class InMemoryDomainEventPublisher : IDomainEventPublisher
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Dictionary<Type, List<Type>> _handlerTypes = new();

    public InMemoryDomainEventPublisher(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : class
        where THandler : IDomainEventHandler<TEvent>
    {
        var t = typeof(TEvent);

        if (!_handlerTypes.TryGetValue(t, out var list))
        {
            list = new List<Type>();
            _handlerTypes[t] = list;
        }

        list.Add(typeof(THandler));
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class
    {
        var t = typeof(TEvent);

        if (!_handlerTypes.TryGetValue(t, out var handlers))
            return;

        var tasks = handlers.Select(async handlerType =>
        {
            using var scope = _scopeFactory.CreateScope();

            var handler = (IDomainEventHandler<TEvent>)scope
                .ServiceProvider
                .GetRequiredService(handlerType);

            await SafeInvoke(() => handler.Handle(@event, ct));
        });

        await Task.WhenAll(tasks);
    }

    private static async Task SafeInvoke(Func<Task> handler)
    {
        try
        {
            await handler().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
