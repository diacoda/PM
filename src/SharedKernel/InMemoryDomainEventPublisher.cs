namespace PM.SharedKernel;

public class InMemoryDomainEventPublisher : IDomainEventPublisher
{
    private readonly Dictionary<Type, List<Func<object, CancellationToken, Task>>> _handlers
        = new();

    public void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : class
    {
        var t = typeof(TEvent);
        if (!_handlers.TryGetValue(t, out var list))
        {
            list = new List<Func<object, CancellationToken, Task>>();
            _handlers[t] = list;
        }

        list.Add((o, ct) => handler((TEvent)o, ct));
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class
    {
        var t = typeof(TEvent);
        if (!_handlers.TryGetValue(t, out var list)) return;
        // fire handlers concurrently but don't block entire method from observing exceptions
        var tasks = list.Select(h => SafeInvoke(h, @event, ct)).ToArray();
        await Task.WhenAll(tasks);
    }

    private static async Task SafeInvoke(Func<object, CancellationToken, Task> handler, object evt, CancellationToken ct)
    {
        try
        {
            await handler(evt, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
