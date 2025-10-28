namespace PM.API.HostedServices;

public class PriceJobOptions
{
    // Example: "America/New_York" - you can extend to use NodaTime for robust timezone handling
    public string TimeZone { get; set; } = "Local";
    // RunTime expressed as hh:mm:ss (configuration binder will bind TimeSpan)
    public TimeSpan RunTime { get; set; } = new TimeSpan(21, 0, 0);
}

