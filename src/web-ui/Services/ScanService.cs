using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using web_ui.Data;

namespace web_ui.Services;

public class ScanService
{
    private ILogger<ScanService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private byte[] start;
    private byte[] end;

    public ScanService(ILogger<ScanService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration.GetSection("Scan");
        start = _configuration["Start"].Split('.').Select(byte.Parse).ToArray();
        end = _configuration["End"].Split('.').Select(byte.Parse).ToArray();
    }

    public async Task<List<DeviceInfo>> AvailableDevices(Action<double>? updateProgress = null)
    {
        var devices = new List<DeviceInfo>();

        async Task NewMethod(byte i, byte j, byte k, byte count, byte s)
        {
            var tasks = new Task[count];
            byte complete = 0;
            for (var l = 0; l < tasks.Length; l++)
            {
                var l1 = l + s;
                tasks[l] = Task.Run(async () =>
                {
                    var ip = new IPAddress(new[] { i, j, k, (byte)l1 });
                    if (await GetDeviceInfo(ip) is { } info)
                        devices.Add(info);
                    complete++;
                    updateProgress?.Invoke(complete / (double)tasks.Length);
                });
            }
            await Task.WhenAll(tasks);
        }

        await AvailableDevices(NewMethod);

        return devices;
    }

    public delegate Task ScanTask(byte i, byte j, byte k, byte count, byte start);

    public async Task AvailableDevices(ScanTask scanner)
    {
        for (var i = start[0]; i <= end[0]; i++)
        {
            for (var j = start[1]; j <= end[1]; j++)
            {
                for (var k = start[2]; k <= end[2]; k++)
                {
                    await scanner(i, j, k, (byte)(end[3] - start[3]), start[3]);
                }
            }
        }
    }

    public async ValueTask<bool> IsAvailable(IPAddress ip)
    {
        var reply = await new Ping().SendPingAsync(ip);
        return reply.Status == IPStatus.Success;
    }
    public async Task<DeviceInfo?> GetDeviceInfo(IPAddress address)
    {
        var reply = await new Ping().SendPingAsync(address);
        _logger.LogTrace("Sending Ping to {Address}", address);
        if (reply.Status != IPStatus.Success)
            return null;
        if (await GetDeviceSpecs(reply.Address) is not { } specs)
            return null;
        _logger.LogInformation("Received specs from {Address}", address);
        return new DeviceInfo(reply.Address, specs.ChipId, specs.Version, reply.Status);
    }

    public async Task<ApiDeviceRoutes?> GetDeviceRoutes(IPAddress address)
    {
        var httpClient = _httpClientFactory.CreateClient();
        try
        {
            var response = await httpClient.GetAsync($"http://{address}/api/io/list");
            return await response.Content.ReadFromJsonAsync<ApiDeviceRoutes>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiDeviceSpecs?> GetDeviceSpecs(IPAddress address)
    {
        var client = new HttpClient();
        try
        {
            //send get request to /api/info

            var uri = $"http://{address}/api/info";
            _logger.LogDebug("Sending Get Request to {Uri}", uri);
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiDeviceSpecs>();
            }
            return null;
        }
        catch (HttpRequestException e)
        {
        }
        catch (Exception e)
        {
            //ignore
        }
        finally
        {
            client.Dispose();
        }
        return null;
    }

    public async Task<List<NetworkDevice>?> ScanNetwork()
    {
        var devices = new List<NetworkDevice>();

        async Task NewMethod(byte i, byte j, byte k, byte count, byte s)
        {
            var tasks = new Task[count];
            for (var l = 0; l < tasks.Length; l++)
            {
                var l1 = l + s;
                tasks[l] = Task.Run(async () =>
                {
                    var ip = new IPAddress(new[] { i, j, k, (byte)l1 });
                    var reply = await new Ping().SendPingAsync(ip);
                    if (reply.Status == IPStatus.Success)
                    {
                        //get device name
                        string hostname = "";
                        try
                        {
                            var host = await Dns.GetHostEntryAsync(reply.Address);

                            hostname = host.HostName;
                        }
                        catch
                        {
                            //ignore    
                        }
                        devices.Add(new NetworkDevice(reply.Address, reply.RoundtripTime, hostname));
                    }
                });
            }
            await Task.WhenAll(tasks);
        }

        await AvailableDevices(NewMethod);

        return devices;
    }

    public async Task<ApiApparatusData?> GetSensorData(IPAddress ip, string deviceRoute, string name, DeviceApparatus deviceApparatus)
    {
        var httpClient = _httpClientFactory.CreateClient();
        try
        {
            var response = await httpClient.GetAsync($"http://{ip}{deviceRoute.Replace("{name}", name)}");
            return await response.Content.ReadFromJsonAsync<ApiApparatusData>();
        }
        catch
        {
            return null;
        }
        
    }

}

public record DeviceInfo(IPAddress Address, string Id, string Version, IPStatus Status);

public record NetworkDevice(IPAddress Address, long RoundtripTime, string HostName);

