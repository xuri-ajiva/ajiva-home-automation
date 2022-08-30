using System.Net;
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using web_ui.Data;
using web_ui.Pages;

namespace web_ui.Services;

public class DeviceService
{
    ApplicationDbContext _context;
    private readonly ScanService _scanService;

    public DeviceService(ApplicationDbContext context, ScanService scanService)
    {
        _context = context;
        _scanService = scanService;
    }

    public async Task<List<Device>> GetDevices()
    {
        return await _context.Devices.ToListAsync();
    }

    public async Task<IndexingResult> DiscoverDevices(Action<double>? updateProgress = null)
    {
        var devices = await _scanService.AvailableDevices(updateProgress);
        int found = 0, @new = 0, updated = 0;
        foreach (var deviceInfo in devices)
        {
            found++;
            var id = deviceInfo.Id;
            if (await _context.Devices
                    .Include(x => x.Apparatus)
                    .FirstOrDefaultAsync(d => d.DeviceId == id) is { } device)
            {
                updated++;
                device.IPAddress = deviceInfo.Address.GetAddressBytes();
                device.Version = deviceInfo.Version;
                device.LastSeen = DateTime.Now;
                await UpdateDeviceApparatus(device, deviceInfo.Address);
                await UpdateDeviceApparatusData(device, deviceInfo.Address);
            }
            else
            {
                @new++;
                var entry = await _context.Devices.AddAsync(new Device {
                    Name = deviceInfo.ToString(),
                    DeviceId = deviceInfo.Id,
                    IPAddress = deviceInfo.Address.GetAddressBytes(),
                    Description = "",
                    Version = deviceInfo.Version,
                    DeviceConfig = new DeviceConfig() {
                        Data = "",
                    },
                    LastSeen = DateTime.Now,
                    Added = DateTime.Now,
                });
                await UpdateDeviceApparatus(entry.Entity, deviceInfo.Address);
            }
        }

        await _context.SaveChangesAsync();
        return new IndexingResult(found, @new, updated);
    }

    private async Task UpdateDeviceApparatus(Device device, IPAddress address)
    {
        var routeData = await _scanService.GetDeviceRoutes(address)
                        ?? new ApiDeviceRoutes { Route = "/invalid" };

        if (string.IsNullOrEmpty(device.Route) || device.Route != routeData.Route)
        {
            device.Route = routeData.Route;
        }

        foreach (var info in routeData.Devices)
        {
            var apparatus = device.Apparatus.FirstOrDefault(x => x.Name == info.Name);
            if (apparatus is not null) continue;
            
            apparatus = new DeviceApparatus {
                Name = info.Name,
                Type = (IoType)info.Type,
                Unit = "n/a"
            };
            device.Apparatus.Add(apparatus);
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task<IndexingResult> PingDevices(Action<double>? updateProgress = null)
    {
        int updated = 0;
        await foreach (var device in _context.Devices
                           .Include(x => x.Apparatus).AsAsyncEnumerable())
        {
            var ip = new IPAddress(device.IPAddress);
            if (!await _scanService.IsAvailable(ip)) continue;

            device.LastSeen = DateTime.Now;
            updated++;

            await UpdateDeviceApparatusData(device, ip);
        }
        await _context.SaveChangesAsync();
        return new IndexingResult(updated, 0, updated);
    }

    private async Task UpdateDeviceApparatusData(Device device, IPAddress ip)
    {
        foreach (var apparatus in device.Apparatus)
        {
            if (apparatus.Type is not (IoType.Pulse or IoType.Read)) continue;
            var rawData = await _scanService.GetSensorData(ip, device.Route, apparatus.Name, apparatus);

            if (rawData is null) continue;
            var apparatusData = new ApparatusData {
                Raw = rawData.Raw,
                Apparatus = apparatus,
            };
            apparatus.Unit = rawData.Unit;
            _context.ApparatusData.Add(apparatusData);
        }
    }

    public async Task UpdateDevice(Device context)
    {
        if (await _context.Devices.FirstOrDefaultAsync(d => d.DeviceId == context.DeviceId) is { } original)
        {
            context.Id = original.Id;
            context.Added = original.Added;
            context.LastSeen = original.LastSeen;
            _context.Devices.Update(context);
            /*_context.Entry(original)
                .CurrentValues.SetValues(context);*/
            await _context.SaveChangesAsync();
        }
        await _context.SaveChangesAsync();
    }

    public async Task<Device?> GetDevice(string deviceId)
    {
        return await _context.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId);
    }
}

public record IndexingResult(int Found, int New, int Updated);
