using Microsoft.Extensions.Options;
using PM.Application.Interfaces;
using PM.Application.Mappers;
using PM.Domain.Events;
using PM.SharedKernel.Events;

namespace PM.API.HostedServices;

/// <summary>
/// Background service that fetches daily market prices.
/// Runs once per trading day, after markets close, with retries until success.
/// Publishes <see cref="DailyPricesFetchedEvent"/> on completion.
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
    /// Constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="options"></param>
    /// <param name="scopeFactory"></param>
    /// <param name="calendar"></param>
    /// <param name="priceFetchedProducer"></param>
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
    /// Main service loop.  
    /// Executes the daily price fetch job, with retries, and schedules the next run.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return;

        _logger.LogInformation(
            "DailyPriceService started. Safe run time: {RunTime}",
            _options.RunTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var lastRun = _statePersistence.LoadLastSuccessfulRun();

                if (lastRun == today)
                {
                    _logger.LogInformation(
                        "Prices already fetched today ({Date}). Waiting for next trading day.",
                        today);

                    await DelayUntilNextMarketDay(stoppingToken);
                    continue;
                }

                if (!_calendar.IsMarketOpen(today))
                {
                    _logger.LogInformation(
                        "Market closed today ({Date}). Waiting until next open market day.",
                        today);

                    await DelayUntilNextMarketDay(stoppingToken);
                    continue;
                }

                await WaitUntilSafeRunTime(stoppingToken);

                await TryFetchPricesForDay(today, stoppingToken);

                await DelayUntilNextMarketDay(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception in DailyPriceService. Retrying in 5 minutes.");

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("DailyPriceService stopping.");
    }

    /// <summary>
    /// Ensures execution occurs only after the configured safe runtime.
    /// </summary>
    private async Task WaitUntilSafeRunTime(CancellationToken token)
    {
        var safeRunTime = DateTime.Today
            .Add(_options.RunTime)
            .AddMinutes(_options.CloseBufferMinutes);

        if (DateTime.Now >= safeRunTime)
            return;

        var delay = safeRunTime - DateTime.Now;

        _logger.LogInformation(
            "Waiting {Minutes} minutes until safe run time ({Time:t})",
            delay.TotalMinutes,
            safeRunTime);

        await Task.Delay(delay, token);
    }

    /// <summary>
    /// Performs price fetching with retry-until-success behavior.
    /// Publishes <see cref="DailyPricesFetchedEvent"/> after each attempt.
    /// </summary>
    private async Task TryFetchPricesForDay(DateOnly date, CancellationToken token)
    {
        var dayEnd = DateTime.Today.AddDays(1);

        using var scope = _scopeFactory.CreateScope();
        var aggregator = scope.ServiceProvider.GetRequiredService<IDailyPriceAggregator>();

        while (!token.IsCancellationRequested && DateTime.Now < dayEnd)
        {
            var startedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Fetching daily prices for {Date} at {Now}.",
                date, DateTime.Now);

            var result = await aggregator.RunOnceAsync(date, allowMarketClosed: false, ct: token);

            var allSucceeded = !result.Errors.Any() && result.Fetched.Any();

            // Build domain event
            var evt = new DailyPricesFetchedEvent(
                effectiveDate: date,
                runTimestamp: startedAt,
                allSucceeded: allSucceeded,
                fetchedCount: result.Fetched.Count,
                skippedCount: result.Skipped.Count,
                errorCount: result.Errors.Count,
                details: SymbolFetchDetailMapper.ToDomainList(result.Details),
                notes: allSucceeded ? "Success" : "Partial/Retry");

            var wrapped = new Event<DailyPricesFetchedEvent>(
                evt,
                new EventMetadata(Guid.NewGuid().ToString()));

            await _priceFetchedProducer.Publish(wrapped, token);

            if (allSucceeded)
            {
                _statePersistence.SaveLastSuccessfulRun(date);

                _logger.LogInformation(
                    "Successfully fetched all prices for {Date}.",
                    date);

                return;
            }

            _logger.LogWarning(
                "Partial results for {Date}. Retrying in {Minutes} minutes. Fetched:{Fetched}, Errors:{Errors}",
                date,
                _options.RetryIntervalMinutes,
                result.Fetched.Count,
                result.Errors.Count);

            await Task.Delay(TimeSpan.FromMinutes(_options.RetryIntervalMinutes), token);
        }

        _logger.LogWarning(
            "Reached end-of-day retry window for {Date}. Continuing tomorrow.",
            date);
    }

    /// <summary>
    /// Uses calendar service to compute next market run time and waits until then.
    /// </summary>
    private async Task DelayUntilNextMarketDay(CancellationToken token)
    {
        var nextRun = _calendar.GetNextMarketRunDateTime(_options.RunTime);

        var delay = nextRun - DateTime.Now;

        _logger.LogInformation(
            "Next fetch scheduled for {NextRun:g} ({Days} days from now).",
            nextRun,
            delay.TotalDays);

        if (delay > TimeSpan.Zero)
            await Task.Delay(delay, token);
    }
}
