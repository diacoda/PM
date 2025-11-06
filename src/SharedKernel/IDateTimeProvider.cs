namespace PM.SharedKernel;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
