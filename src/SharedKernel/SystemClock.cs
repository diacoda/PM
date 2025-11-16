namespace PM.SharedKernel;

public class SystemClock : ISystemClock
{
    public DateTime Now => DateTime.Now;
}