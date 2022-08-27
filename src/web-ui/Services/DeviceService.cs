using System.Net;
using Microsoft.EntityFrameworkCore;
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

    public async Task<IndexingResult> IndexNetworkDevices(Action<double>? updateProgress = null)
    {
        var devices = await _scanService.AvailableDevices(updateProgress);
        int found = 0, @new = 0, updated = 0;
        foreach (var deviceInfo in devices)
        {
            found++;
            var id = deviceInfo.Id;
            if (await _context.Devices.FirstOrDefaultAsync(d => d.DeviceId == id) is { } device)
            {
                updated++;
                device.IPAddress = deviceInfo.Address.GetAddressBytes();
                device.Version = deviceInfo.Version;
                device.LastSeen = DateTime.Now;
            }
            else
            {
                @new++;
                await _context.Devices.AddAsync(new Device() {
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
            }
        }

        await _context.SaveChangesAsync();
        return new IndexingResult(found, @new, updated);
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
