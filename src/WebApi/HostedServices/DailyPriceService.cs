// DailyPriceService.cs
using Microsoft.Extensions.Options;
using PM.Application.Interfaces;
using PM.Application.Mappers;
using PM.Domain.Events;
using PM.SharedKernel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PM.API.HostedServices;

public class DailyPriceService : BackgroundService
{
    private readonly ILogger<DailyPriceService> _logger;
    private readonly PriceJobOptions _options;
    private readonly IDailyPriceAggregator _aggregator;
    private readonly IMarketCalendar _calendar;
    private readonly IDomainEventPublisher _publisher;
    private readonly StatePersistence _statePersistence;

    public DailyPriceService(
        ILogger<DailyPriceService> logger,
        IOptions<PriceJobOptions> options,
        IDailyPriceAggregator aggregator,
        IMarketCalendar calendar,
        IDomainEventPublisher publisher)
    {
        _logger = logger;
        _options = options.Value;
        _aggregator = aggregator;
        _calendar = calendar;
        _publisher = publisher;
        _statePersistence = new StatePersistence(_options.StateFilePath);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("📈 DailyPriceService started. RunTime: {RunTime}", _options.RunTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var lastRun = _statePersistence.LoadLastSuccessfulRun();

                if (lastRun == today)
                {
                    _logger.LogInformation("Prices already fetched today ({Date}). Waiting until next market day.", today);
                    await DelayUntilNextMarketDay(stoppingToken);
                    continue;
                }

                // If markets closed today -> skip until next market day
                if (!_calendar.IsMarketOpen(today))
                {
                    _logger.LogInformation("Market closed on {Date}. Waiting until next market day.", today);
                    await DelayUntilNextMarketDay(stoppingToken);
                    continue;
                }

                // Wait until configured runtime + buffer (as "technically safe" time).
                var safeRunDateTime = DateTime.Today.Add(_options.RunTime).AddMinutes(_options.CloseBufferMinutes);
                var now = DateTime.Now;
                if (now < safeRunDateTime)
                {
                    var wait = safeRunDateTime - now;
                    _logger.LogInformation("Waiting {WaitMinutes} minutes until safe run time ({SafeTime:t}).", wait.TotalMinutes, safeRunDateTime);
                    await Task.Delay(wait, stoppingToken);
                }

                // Attempt fetches repeatedly on the same trading day until success or until day ends.
                var dayEnd = DateTime.Today.AddDays(1); // midnight local
                bool success = false;

                while (!stoppingToken.IsCancellationRequested && DateTime.Now < dayEnd)
                {
                    var startedAt = DateTime.UtcNow;
                    _logger.LogInformation("Attempting fetch for {Date} at {Now}", today, DateTime.Now);
                    var result = await _aggregator.RunOnceAsync(today, allowMarketClosed: false, ct: stoppingToken);

                    // Determine success by absence of errors and at least one fetched price
                    var allSucceeded = !result.Errors.Any() && result.Fetched.Any();

                    // Publish domain event with full details
                    var evt = new DailyPricesFetchedEvent(
                        effectiveDate: today,
                        runTimestamp: startedAt,
                        allSucceeded: allSucceeded,
                        fetchedCount: result.Fetched.Count,
                        skippedCount: result.Skipped.Count,
                        errorCount: result.Errors.Count,
                        details: SymbolFetchDetailMapper.ToDomainList(result.Details),
                        notes: allSucceeded ? "Success" : "Partial/Retry"
                    );

                    // publish without waiting for handlers (but await the Publish call)
                    await _publisher.PublishAsync(evt, stoppingToken);

                    if (allSucceeded)
                    {
                        _statePersistence.SaveLastSuccessfulRun(today);
                        _logger.LogInformation("✅ Successfully fetched prices for {Date}", today);
                        success = true;
                        break;
                    }

                    // Not all available yet -> retry after configured interval
                    _logger.LogWarning("⚠️ Not all prices ready for {Date}, retrying in {Minutes} minutes. Fetched:{Fetched}, Errors:{Errors}",
                        today, _options.RetryIntervalMinutes, result.Fetched.Count, result.Errors.Count);

                    await Task.Delay(TimeSpan.FromMinutes(_options.RetryIntervalMinutes), stoppingToken);

                    // If markets close earlier than midnight, you could bail earlier by checking calendar.IsAfterMarketClose for all exchanges here.
                    // For simplicity, we continue until midnight local time.
                }

                // Either succeeded or out of time for today -> wait until next market day
                await DelayUntilNextMarketDay(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break; // graceful
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in DailyPriceService loop.");
                // Backoff
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("🛑 DailyPriceService stopping.");
    }

    private async Task DelayUntilNextMarketDay(CancellationToken token)
    {
        var candidate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        while (!_calendar.IsMarketOpen(candidate))
            candidate = candidate.AddDays(1);

        var runTime = TimeOnly.FromTimeSpan(_options.RunTime);
        var nextRun = candidate.ToDateTime(runTime);
        var delay = nextRun - DateTime.Now;

        _logger.LogInformation("⏳ Next run scheduled for {NextRun:g} ({Days} days from now)", nextRun, delay.TotalDays);

        if (delay > TimeSpan.Zero)
            await Task.Delay(delay, token);
    }
}
