namespace web_ui.Services;

public class BackgroundDiscoverService : BackgroundService
{
    private readonly ILogger<BackgroundDiscoverService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    /// <inheritdoc />
    public BackgroundDiscoverService(
        ILogger<BackgroundDiscoverService> logger,
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
        var timer = new PeriodicTimer(TimeSpan.Parse(_configuration["BackgroundService:DiscoverInterval"]));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var scope = _serviceProvider.CreateScope();
            var deviceService = scope.ServiceProvider
                .GetRequiredService<DeviceService>();

            var result = await deviceService.DiscoverDevices();

            _logger.LogInformation("Scanned Network, Found {Found} Devices, New {New}, Updated {Updated}",
                result.Found, result.New, result.Updated);
        }
    }
}
