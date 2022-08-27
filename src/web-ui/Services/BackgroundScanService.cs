namespace web_ui.Services;

public class BackgroundScanService : BackgroundService
{
    private readonly ILogger<BackgroundScanService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    /// <inheritdoc />
    public BackgroundScanService(
        ILogger<BackgroundScanService> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.Parse(_configuration["BackgroundScanService:ScanInterval"]));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var scope = _serviceProvider.CreateScope();
            var result = await scope.ServiceProvider
                .GetRequiredService<DeviceService>()
                .IndexNetworkDevices();
            _logger.LogInformation("Scanned Network, Found {Found} Devices, New {New}, Updated {Updated}",
                result.Found, result.New, result.Updated);
        }
    }
}
