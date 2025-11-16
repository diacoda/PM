namespace PM.SharedKernel.Events;
/// <summary>
/// Starts processing our bus
/// We can manipulate Subscribe and Unsubscribe methods to
/// turn processing on or off
/// </summary>
public interface IConsumer : IAsyncDisposable
{
    ValueTask Start(CancellationToken token = default);

    ValueTask Stop(CancellationToken token = default);
}

public interface IConsumer<T> : IConsumer
{
}