namespace PM.API.HostedServices;

/// <summary>
/// Configuration options for the DailyPriceService background job.
/// </summary>
public class PriceJobOptions
{
    /// <summary>
    /// Time zone identifier (e.g., "America/Toronto").
    /// </summary>
    public string TimeZone { get; set; } = "Local";

    /// <summary>
    /// Time of day to start fetching prices (usually after market close).
    /// </summary>
    public TimeSpan RunTime { get; set; } = new TimeSpan(18, 0, 0);

    /// <summary>
    /// How often to retry fetching prices (in minutes) when data is not yet available.
    /// </summary>
    public int RetryIntervalMinutes { get; set; } = 30;
}
