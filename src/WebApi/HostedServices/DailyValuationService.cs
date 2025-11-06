
using PM.Application.Interfaces;

namespace PM.API.HostedServices;

public class DailyValuationService : BackgroundService
{
    private readonly ILogger<DailyValuationService> _logger;
    private readonly IMarketCalendar _marketCalendar;
    private readonly IServiceScopeFactory _scopeFactory;

    public DailyValuationService(
        ILogger<DailyValuationService> logger,
        IMarketCalendar marketCalendar,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _marketCalendar = marketCalendar;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily Valuation Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var today = DateTime.Today;
            var dateOnly = DateOnly.FromDateTime(today);

            if (_marketCalendar.IsMarketOpen(dateOnly) || true)
            {
                using var scope = _scopeFactory.CreateScope();
                var valuationScheduler = scope.ServiceProvider.GetRequiredService<IValuationScheduler>();
                var valuationCalculator = scope.ServiceProvider.GetRequiredService<IValuationCalculator>();

                var periodsToRun = valuationScheduler.GetValuationsForToday(today);

                _logger.LogInformation("Calculating valuations for {Date} for periods: {Periods}", dateOnly, string.Join(", ", periodsToRun));

                try
                {
                    await valuationCalculator.CalculateValuationsAsync(dateOnly, periodsToRun, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating valuations for {Date}", today);
                }
            }

            var nextRunTime = DateTime.Today.AddDays(1).AddHours(2);
            var delay = nextRunTime - DateTime.Now;

            if (delay.TotalMilliseconds > 0)
                await Task.Delay(delay, stoppingToken);
        }
    }
}
