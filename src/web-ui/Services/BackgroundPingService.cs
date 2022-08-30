namespace web_ui.Services;

public class BackgroundPingService : BackgroundService
{
    private readonly ILogger<BackgroundPingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    /// <inheritdoc />
    public BackgroundPingService(
        ILogger<BackgroundPingService> logger,
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
        var timer = new PeriodicTimer(TimeSpan.Parse(_configuration["BackgroundService:PingInterval"]));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var scope = _serviceProvider.CreateScope();
            var deviceService = scope.ServiceProvider
                .GetRequiredService<DeviceService>();

            var result = await deviceService.PingDevices();

            _logger.LogInformation("Pinged Devices: {Count}", result.Updated);
        }
    }
}
