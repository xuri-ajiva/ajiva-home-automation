using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace web_ui.Services;

public class ScanService
{
    private ILogger<ScanService> _logger;

    public ScanService(ILogger<ScanService> logger)
    {
        _logger = logger;
    }

    public async Task<List<DeviceInfo>> AvailableDevices(IPAddress from, IPAddress to)
    {
        var devices = new List<DeviceInfo>();
        //ping all IP addresses in the range and add them to the list

        var start = from.GetAddressBytes();
        var end = to.GetAddressBytes();

        for (var i = start[0]; i <= end[0]; i++)
        {
            for (var j = start[1]; j <= end[1]; j++)
            {
                for (var k = start[2]; k <= end[2]; k++)
                {
                    var i1 = i;
                    var j1 = j;
                    var k1 = k;
                    var tasks = new Task[end[3] - start[3]];
                    for (var l = 0; l < tasks.Length; l++)
                    {
                        var l1 = l + start[3];
                        tasks[l] = Task.Run(async () =>
                        {
                            var ip = new IPAddress(new[] { i1, j1, k1, (byte)l1 });
                            if (await GetDeviceInfo(ip) is { } info)
                                devices.Add(info);
                        });
                    }
                    await Task.WhenAll(tasks);
                }
            }
        }

        return devices;
    }

    public async Task<DeviceInfo?> GetDeviceInfo(IPAddress address)
    {
        var reply = await new Ping().SendPingAsync(address);
        _logger.LogTrace("Sending Ping to {Address}", address);
        if (reply.Status != IPStatus.Success)
            return null;
        if (await GetDeviceSpecs(reply.Address) is not { } specData)
            return null;
        if (JsonSerializer.Deserialize<DeviceSpecs>(specData) is not { } specs)
            return null;
        _logger.LogInformation("Received specs from {Address}", address);
        return new DeviceInfo(reply.Address, specs.ChipId, specs.Version, reply.Status);
    }

    public async Task<string?> GetDeviceSpecs(IPAddress address)
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
                var content = await response.Content.ReadAsStringAsync();
                return content;
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
}

public record DeviceInfo(IPAddress Address, string Id, string Version, IPStatus Status);

public class DeviceSpecs
{
    [JsonPropertyName("chipId")]
    public string ChipId { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }
}
