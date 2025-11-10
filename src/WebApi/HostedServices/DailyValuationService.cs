using PM.Application.Interfaces;

namespace PM.API.HostedServices;

/// <summary>
/// A background service that automatically performs daily portfolio valuations.
/// </summary>
/// <remarks>
/// This hosted service runs once per day (at approximately 2 AM) and triggers
/// valuation calculations for all applicable portfolios, based on the current market date
/// and business calendar provided by <see cref="IMarketCalendar"/>.
/// </remarks>
public class DailyValuationService : BackgroundService
{
    private readonly ILogger<DailyValuationService> _logger;
    private readonly IMarketCalendar _marketCalendar;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DailyValuationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for diagnostic messages.</param>
    /// <param name="marketCalendar">The market calendar used to determine open trading days.</param>
    /// <param name="scopeFactory">
    /// The service scope factory used to create a scoped service provider
    /// for resolving valuation services.
    /// </param>
    public DailyValuationService(
        ILogger<DailyValuationService> logger,
        IMarketCalendar marketCalendar,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _marketCalendar = marketCalendar;
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Executes the background valuation process.
    /// </summary>
    /// <param name="stoppingToken">A token used to signal service cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// The service checks whether the market is open (or proceeds regardless if configured)
    /// and performs daily valuations for all relevant periods using injected valuation services.
    /// The process repeats every 24 hours, starting approximately at 2 AM system time.
    /// </remarks>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily Valuation Service started.");
        return;
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

                _logger.LogInformation(
                    "Calculating valuations for {Date} for periods: {Periods}",
                    dateOnly,
                    string.Join(", ", periodsToRun));

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
