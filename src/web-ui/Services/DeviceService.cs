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

    public async Task<List<DeviceInfo>> IndexNetworkDevices(string from, string to)
    {
        var devices = await _scanService.AvailableDevices(IPAddress.Parse(from), IPAddress.Parse(to));

        foreach (var deviceInfo in devices)
        {
            await _context.Devices.AddAsync(new Device()
            {
                Name = deviceInfo.ToString(),
                DeviceId = deviceInfo.Id,
                IPAddress = deviceInfo.Address.GetAddressBytes(),
                Description = "",
                DeviceConfig = new DeviceConfig()
                {
                    Data = ""
                }
            });
        }

        await _context.SaveChangesAsync();
        return devices;
    }
}
