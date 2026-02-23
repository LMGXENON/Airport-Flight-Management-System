using AFMS.Services;

namespace AFMS.BackgroundServices;

public class FlightUpdateBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FlightUpdateBackgroundService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(2); // Update every 2 minutes

    public FlightUpdateBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<FlightUpdateBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Flight Update Background Service started");

        // Wait 10 seconds before first sync to allow app to fully start
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting flight sync...");
                
                using var scope = _serviceProvider.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<FlightSyncService>();
                
                await syncService.SyncFlightsAsync();
                
                _logger.LogInformation($"Flight sync completed. Next sync in {_updateInterval.TotalMinutes} minutes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in flight update background service");
            }

            await Task.Delay(_updateInterval, stoppingToken);
        }

        _logger.LogInformation("Flight Update Background Service stopped");
    }
}
