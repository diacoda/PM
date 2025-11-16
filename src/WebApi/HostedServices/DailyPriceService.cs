using Microsoft.Extensions.Options;
using PM.Application.Interfaces;
using PM.Application.Mappers;
using PM.Domain.Events;
using PM.SharedKernel;
using PM.SharedKernel.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PM.API.HostedServices;

/// <summary>
/// Hosted background service responsible for fetching daily market prices.
///
/// <para>
/// The service executes once per trading day, at a configured "safe" time after markets close.
/// It performs retries until all price fetchers succeed, publishes a
/// <see cref="DailyPricesFetchedEvent"/>, and records the last successful run.
/// </para>
///
/// <para>
/// Logic overview:
/// <list type="number">
///     <item>Ensures it runs only once per trading day.</item>
///     <item>Skips days when markets are closed.</item>
///     <item>Waits until a configured run time (e.g., after market close).</item>
///     <item>Executes the price aggregator, optionally retrying until success or midnight.</item>
///     <item>Publishes domain events for downstream workflows.</item>
///     <item>Persists the successful run date.</item>
///     <item>Sleeps until the next valid market day.</item>
/// </list>
/// </para>
/// </summary>
public class DailyPriceService : BackgroundService
{
    private readonly ILogger<DailyPriceService> _logger;
    private readonly PriceJobOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMarketCalendar _calendar;
    private readonly StatePersistence _statePersistence;
    private readonly IProducer<DailyPricesFetchedEvent> _priceFetchedProducer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DailyPriceService"/> class.
    /// </summary>
    /// <param name="logger">Application logger for diagnostics.</param>
    /// <param name="options">Application options containing runtime settings (schedule, retry intervals, state file paths).</param>
    /// <param name="scopeFactory">.</param>
    /// <param name="calendar">Market calendar service for determining trading days and open/close status.</param>
    /// <param name="priceFetchedProducer">Domain event publisher for broadcasting price fetch results.</param>
    public DailyPriceService(
        ILogger<DailyPriceService> logger,
        IOptions<PriceJobOptions> options,
        IServiceScopeFactory scopeFactory,
        IMarketCalendar calendar,
        IProducer<DailyPricesFetchedEvent> priceFetchedProducer)
    {
        _logger = logger;
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _calendar = calendar;
        _priceFetchedProducer = priceFetchedProducer;
        _statePersistence = new StatePersistence(_options.StateFilePath);
    }

    /// <summary>
    /// Main execution loop of the background service.
    ///
    /// <para>
    /// The method repeats indefinitely until the host shuts down.  
    /// On each iteration:
    /// <list type="number">
    ///     <item>Verifies whether prices were already fetched today.</item>
    ///     <item>Skips execution on non-trading days.</item>
    ///     <item>Waits until a configured safe runtime (post-close).</item>
    ///     <item>Attempts price fetching, with retries until success or the day ends.</item>
    ///     <item>Publishes a domain event for subscribers.</item>
    ///     <item>Sleeps until the next trading day.</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="stoppingToken">Token used to cancel background work when the host shuts down.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyPriceService started. RunTime: {RunTime}", _options.RunTime);

        using var scope = _scopeFactory.CreateScope();
        var aggregator = scope.ServiceProvider.GetRequiredService<IDailyPriceAggregator>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var lastRun = _statePersistence.LoadLastSuccessfulRun();

                // Already done today
                if (lastRun == today)
                {
                    _logger.LogInformation("Prices already fetched today ({Date}). Waiting until next market day.", today);
                    await DelayUntilNextMarketDay(stoppingToken);
                    continue;
                }

                // Skip if market closed
                if (!_calendar.IsMarketOpen(today))
                {
                    _logger.LogInformation("Market closed on {Date}. Waiting until next market day.", today);
                    await DelayUntilNextMarketDay(stoppingToken);
                    continue;
                }

                // Wait until configured time (e.g., 17:00) + buffer
                var safeRunDateTime = DateTime.Today
                    .Add(_options.RunTime)
                    .AddMinutes(_options.CloseBufferMinutes);

                var now = DateTime.Now;
                if (now < safeRunDateTime)
                {
                    var wait = safeRunDateTime - now;
                    _logger.LogInformation(
                        "Waiting {WaitMinutes} minutes until safe run time ({SafeTime:t}).",
                        wait.TotalMinutes, safeRunDateTime);

                    await Task.Delay(wait, stoppingToken);
                }

                // Retry window is until midnight local time
                var dayEnd = DateTime.Today.AddDays(1);


                // Attempts price fetching multiple times until:
                // * All prices succeed, OR
                // * Midnight is reached
                while (!stoppingToken.IsCancellationRequested && DateTime.Now < dayEnd)
                {
                    var startedAt = DateTime.UtcNow;
                    _logger.LogInformation("Attempting fetch for {Date} at {Now}", today, DateTime.Now);


                    // Fetch via aggregator
                    var result = await aggregator.RunOnceAsync(
                        today,
                        allowMarketClosed: false,
                        ct: stoppingToken);

                    // Success = no errors and at least one price returned
                    var allSucceeded = !result.Errors.Any() && result.Fetched.Any();

                    // Build and publish event
                    var dailyPricesFetchedEvent = new DailyPricesFetchedEvent(
                        effectiveDate: today,
                        runTimestamp: startedAt,
                        allSucceeded: allSucceeded,
                        fetchedCount: result.Fetched.Count,
                        skippedCount: result.Skipped.Count,
                        errorCount: result.Errors.Count,
                        details: SymbolFetchDetailMapper.ToDomainList(result.Details),
                        notes: allSucceeded ? "Success" : "Partial/Retry"
                    );

                    var evt = new Event<DailyPricesFetchedEvent>(dailyPricesFetchedEvent, new EventMetadata(Guid.NewGuid().ToString()));
                    await _priceFetchedProducer.Publish(evt, stoppingToken);

                    if (allSucceeded)
                    {
                        _statePersistence.SaveLastSuccessfulRun(today);
                        _logger.LogInformation("Successfully fetched prices for {Date}", today);
                        break;
                    }

                    _logger.LogWarning(
                        "Not all prices ready for {Date}, retrying in {Minutes} minutes. Fetched:{Fetched}, Errors:{Errors}",
                        today,
                        _options.RetryIntervalMinutes,
                        result.Fetched.Count,
                        result.Errors.Count);

                    await Task.Delay(
                        TimeSpan.FromMinutes(_options.RetryIntervalMinutes),
                        stoppingToken);
                }

                // Either success or the day has ended
                await DelayUntilNextMarketDay(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in DailyPriceService loop.");

                // Small backoff before retrying loop
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("DailyPriceService stopping.");
    }

    /// <summary>
    /// Calculates the next market day (skipping weekends/holidays)
    /// and delays execution until that day's configured run time.
    /// </summary>
    /// <param name="token">Cancellation token used by the host.</param>
    private async Task DelayUntilNextMarketDay(CancellationToken token)
    {
        // Move to next day
        var candidate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        // Skip until open market day
        while (!_calendar.IsMarketOpen(candidate))
            candidate = candidate.AddDays(1);

        var runTime = TimeOnly.FromTimeSpan(_options.RunTime);
        var nextRun = candidate.ToDateTime(runTime);

        var delay = nextRun - DateTime.Now;

        _logger.LogInformation(
            "⏳ Next run scheduled for {NextRun:g} ({Days} days from now)",
            nextRun,
            delay.TotalDays);

        if (delay > TimeSpan.Zero)
            await Task.Delay(delay, token);
    }
}
