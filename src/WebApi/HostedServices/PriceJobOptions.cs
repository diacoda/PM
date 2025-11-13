namespace PM.API.HostedServices;

/// <summary>
/// Configuration options for the DailyPriceService background job.
/// </summary>
public class PriceJobOptions
{
    /// <summary>
    /// The time of day to attempt fetching daily prices (wall-clock local time).
    /// Example: 18:30:00 (6:30pm)
    /// </summary>
    public TimeSpan RunTime { get; set; } = TimeSpan.FromHours(18.5);

    /// <summary>
    /// How many minutes between retries during the same market day if not all prices are available.
    /// </summary>
    public int RetryIntervalMinutes { get; set; } = 10;

    /// <summary>
    /// Safety buffer in minutes after RunTime to allow exchange prints to settle, e.g. 5-30 minutes.
    /// </summary>
    public int CloseBufferMinutes { get; set; } = 10;

    /// <summary>
    /// Path for storing last-run state file (optional).
    /// </summary>
    public string StateFilePath { get; set; } = "last_run.json";
}
