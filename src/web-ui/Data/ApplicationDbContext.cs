using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace web_ui.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Device> Devices { get; set; }
    public DbSet<DeviceConfig> DeviceConfigs { get; set; } 
}

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DeiveId { get; set; }
    public int IPAddress { get; set; }
    public DeviceConfig DeviceConfig { get; set; }
}

public class DeviceConfig
{
    public int Id { get; set; }
    public string Data { get; set; }
}
