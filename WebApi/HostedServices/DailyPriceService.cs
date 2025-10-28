using Microsoft.Extensions.Options;
using PM.Application.Commands;

namespace PM.API.HostedServices;

public class DailyPriceService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DailyPriceService> _logger;
    private readonly PriceJobOptions _options;

    public DailyPriceService(IServiceProvider services, ILogger<DailyPriceService> logger, IOptions<PriceJobOptions> options)
    {
        _services = services;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyPriceJob started. RunTime: {runTime}", _options.RunTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var fetcher = scope.ServiceProvider.GetRequiredService<FetchDailyPricesCommand>();

                var today = DateOnly.FromDateTime(DateTime.Today);
                _logger.LogInformation("Running scheduled price fetch for {date}", today);

                try
                {
                    var result = await fetcher.ExecuteAsync(today, allowMarketClosed: false);

                    _logger.LogInformation("Fetch completed: {fetched} fetched, {skipped} skipped, {errors} errors",
                        result.Fetched.Count, result.Skipped.Count, result.Errors.Count);

                    if (result.Errors.Any())
                    {
                        foreach (var err in result.Errors)
                            _logger.LogWarning("Fetch error: {error}", err);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during FetchDailyPricesCommand execution");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (TaskCanceledException) { /* shutting down */ }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in DailyPriceJob main loop");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        _logger.LogInformation("DailyPriceJob stopping");
    }
}
