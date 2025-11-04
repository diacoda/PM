using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PM.Application.Interfaces;

namespace PM.Application.Services;

public class DailyValuationService : BackgroundService
{
    private readonly ILogger<DailyValuationService> _logger;
    private readonly IValuationCalculator _valuationCalculator;
    private readonly ValuationScheduler _valuationScheduler;
    private readonly IMarketCalendar _marketCalendar;

    public DailyValuationService(
        ILogger<DailyValuationService> logger,
        IValuationCalculator valuationCalculator,
        ValuationScheduler valuationScheduler,
        IMarketCalendar marketCalendar)
    {
        _logger = logger;
        _valuationCalculator = valuationCalculator;
        _valuationScheduler = valuationScheduler;
        _marketCalendar = marketCalendar;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily Valuation Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var today = DateTime.Today;
            var dateOnly = DateOnly.FromDateTime(today);

            // Only run if the market is open or if you want to include weekends/holidays
            if (_marketCalendar.IsMarketOpen(dateOnly) || true) // remove '|| true' if you skip weekends
            {
                var periodsToRun = _valuationScheduler.GetValuationsForToday(today);

                foreach (var period in periodsToRun)
                {
                    _logger.LogInformation("Calculating {Period} valuation for {Date}", period, today);
                    try
                    {
                        await _valuationCalculator.CalculateValuationAsync(today, period, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error calculating {Period} valuation for {Date}", period, today);
                    }
                }
            }

            // Wait until the next day at a specific time (e.g., 02:00 AM)
            var nextRunTime = DateTime.Today.AddDays(1).AddHours(2);
            var delay = nextRunTime - DateTime.Now;

            if (delay.TotalMilliseconds > 0)
                await Task.Delay(delay, stoppingToken);
        }
    }
}
