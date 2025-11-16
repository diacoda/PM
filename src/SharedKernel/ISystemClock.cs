namespace PM.SharedKernel;

public interface ISystemClock
{
    DateTime Now { get; }
}