namespace PM.SharedKernel.Events;
/// <summary>
/// Handles incoming event
/// </summary>
public interface IEventHandler<in T>
{
    ValueTask Handle(T? time, CancellationToken token = default);
}