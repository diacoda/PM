using PM.Application.Interfaces;

namespace PM.API.HostedServices;

/// <summary>
/// Background service that runs daily portfolio valuations.  
/// Executes once per day at a configured time (default 2 AM).
/// </summary>
public class DailyValuationService : BackgroundService
{
    private readonly ILogger<DailyValuationService> _logger;
    private readonly IMarketCalendar _calendar;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _runTime = new(2, 0, 0); // 2 AM
    private readonly bool _requireMarketOpen = true; // configurable: run even when market closed

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="calendar"></param>
    /// <param name="scopeFactory"></param>
    public DailyValuationService(
        ILogger<DailyValuationService> logger,
        IMarketCalendar calendar,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _calendar = calendar;
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Daily valuation functions
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "DailyValuationService started. Run time = {Time}, RequireMarketOpen={Require}",
            _runTime,
            _requireMarketOpen);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunValuationsIfDue(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break; // graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception in DailyValuationService.");
            }

            await DelayUntilNextValuationDay(stoppingToken);
        }

        _logger.LogInformation("DailyValuationService stopping.");
    }

    /// <summary>
    /// Executes valuations for today's valuation date.
    /// </summary>
    private async Task RunValuationsIfDue(CancellationToken token)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var targetDate =
            _calendar.GetNextValuationDate(today, _requireMarketOpen);

        if (targetDate != today)
        {
            _logger.LogInformation(
                "Today ({Today}) is not valuation day. Next valuation date: {Target}",
                today, targetDate);
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<IValuationScheduler>();
        var calculator = scope.ServiceProvider.GetRequiredService<IValuationCalculator>();

        var periods = scheduler.GetValuationsForToday(DateTime.Today);

        _logger.LogInformation(
            "Starting valuations for {Date}. Periods: {Periods}",
            targetDate,
            string.Join(", ", periods));

        try
        {
            await calculator.CalculateValuationsAsync(targetDate, periods, token);

            _logger.LogInformation(
                "Completed valuations for {Date}.",
                targetDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error calculating valuations for {Date}.",
                targetDate);
        }
    }

    /// <summary>
    /// Uses IMarketCalendar to determine the next valuation run datetime.
    /// </summary>
    private async Task DelayUntilNextValuationDay(CancellationToken token)
    {
        var nextRun = _calendar.GetNextValuationRunDateTime(_runTime, _requireMarketOpen);

        var delay = nextRun - DateTime.Now;

        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;

        _logger.LogInformation(
            "Next valuation run scheduled for {NextRun} (in {Hours} hours).",
            nextRun,
            delay.TotalHours);

        await Task.Delay(delay, token);
    }
}
