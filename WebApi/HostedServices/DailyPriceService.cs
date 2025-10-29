using Microsoft.Extensions.Options;
using PM.Application.Commands;
using PM.Application.Interfaces;
using System.Text.Json;

namespace PM.API.HostedServices;

/// <summary>
/// Background service responsible for fetching daily market prices
/// once markets are closed. Retries same-day until success, then waits
/// until the next market open day.
/// </summary>
public class DailyPriceService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DailyPriceService> _logger;
    private readonly PriceJobOptions _options;
    private readonly IMarketCalendar _calendar;

    private const string StateFilePath = "last_run.json";

    /// <summary>
    /// Daily price service constructor
    /// </summary>
    /// <param name="services"></param>
    /// <param name="logger"></param>
    /// <param name="options"></param>
    /// <param name="calendar"></param>
    public DailyPriceService(
        IServiceProvider services,
        ILogger<DailyPriceService> logger,
        IOptions<PriceJobOptions> options,
        IMarketCalendar calendar)
    {
        _services = services;
        _logger = logger;
        _options = options.Value;
        _calendar = calendar;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("📈 DailyPriceJob started. RunTime: {runTime}", _options.RunTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var lastRun = LoadLastRun();

                // Skip if already successfully fetched today
                if (lastRun == today)
                {
                    _logger.LogInformation("Prices already fetched today ({Date}), waiting for next market day.", today);
                    await DelayUntilNextMarketDay(stoppingToken);
                    continue;
                }

                if (!_calendar.IsMarketOpen(today))
                {
                    _logger.LogInformation("Market closed today ({Date}), waiting for next market day.", today);
                    await DelayUntilNextMarketDay(stoppingToken);
                    continue;
                }

                if (!_calendar.IsAfterMarketClose("TSX"))
                {
                    var nextRunTime = DateTime.Today.Add(_options.RunTime);
                    var wait = nextRunTime - DateTime.Now;
                    if (wait > TimeSpan.Zero)
                    {
                        _logger.LogInformation("Waiting until market close ({NextRunTime:t}) to fetch prices...", nextRunTime);
                        await Task.Delay(wait, stoppingToken);
                    }
                }

                bool success = await TryFetchPrices(today, stoppingToken);

                if (success)
                {
                    SaveLastRun(today);
                    _logger.LogInformation("✅ Successfully fetched prices for {Date}", today);
                    await DelayUntilNextMarketDay(stoppingToken);
                }
                else
                {
                    _logger.LogWarning("⚠️ Prices not ready yet for {Date}, retrying in {RetryMinutes} minutes.",
                        today, _options.RetryIntervalMinutes);
                    await Task.Delay(TimeSpan.FromMinutes(_options.RetryIntervalMinutes), stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                // Graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in DailyPriceJob loop.");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("🛑 DailyPriceJob stopping.");
    }

    /// <summary>
    /// Executes the FetchDailyPricesCommand for the given date.
    /// </summary>
    private async Task<bool> TryFetchPrices(DateOnly date, CancellationToken token)
    {
        try
        {
            using var scope = _services.CreateScope();
            var fetcher = scope.ServiceProvider.GetRequiredService<FetchDailyPricesCommand>();

            var result = await fetcher.ExecuteAsync(date, allowMarketClosed: false);

            _logger.LogInformation("Fetch completed: {Fetched} fetched, {Skipped} skipped, {Errors} errors",
                result.Fetched.Count, result.Skipped.Count, result.Errors.Count);

            if (result.Errors.Any())
            {
                foreach (var err in result.Errors)
                    _logger.LogWarning("Fetch error: {Error}", err);
            }

            return result.Fetched.Count > 0 && !result.Errors.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during FetchDailyPricesCommand execution.");
            return false;
        }
    }

    /// <summary>
    /// Waits until the next market open day after today.
    /// </summary>
    private async Task DelayUntilNextMarketDay(CancellationToken token)
    {
        var next = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        while (!_calendar.IsMarketOpen(next))
            next = next.AddDays(1);

        var runTime = TimeOnly.FromTimeSpan(_options.RunTime);
        var nextRunTime = next.ToDateTime(runTime);
        var delay = nextRunTime - DateTime.Now;

        _logger.LogInformation("⏳ Next run scheduled for {NextRunDateTime:g} ({Days} days from now)",
            nextRunTime, delay.TotalDays);

        if (delay > TimeSpan.Zero)
            await Task.Delay(delay, token);
    }

    /// <summary>
    /// Saves the last successful run date to a local JSON file.
    /// </summary>
    private void SaveLastRun(DateOnly date)
    {
        var json = JsonSerializer.Serialize(date);
        File.WriteAllText(StateFilePath, json);
    }

    /// <summary>
    /// Loads the last successful run date from local JSON file.
    /// </summary>
    private DateOnly? LoadLastRun()
    {
        if (!File.Exists(StateFilePath))
            return null;

        try
        {
            var json = File.ReadAllText(StateFilePath);
            return JsonSerializer.Deserialize<DateOnly>(json);
        }
        catch
        {
            return null;
        }
    }
}
