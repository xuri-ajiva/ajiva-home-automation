using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
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
    public DbSet<DeviceApparatus> DeviceApparatus { get; set; }
    public DbSet<ApparatusData> ApparatusData { get; set; }
}
public class Device
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DeviceId { get; set; }
    public byte[] IPAddress { get; set; }
    public DeviceConfig DeviceConfig { get; set; }
    public string Version { get; set; }
    public DateTime LastSeen { get; set; }
    public DateTime Added { get; set; }
    
    
    public string Route { get; set; }
    [InverseProperty(nameof(DeviceApparatus.Device))]
    public ICollection<DeviceApparatus> Apparatus { get; set; }
}
public enum IoType
{
    Write = 1,
    Read = 2,
    Pulse = 3,
}
public class DeviceApparatus
{
    public int Id { get; set; }

    public string Name { get; set; }
    public IoType Type { get; set; }
    public string Unit { get; set; }

    public string? Description { get; set; }
    public Device Device { get; set; }
    
    [InverseProperty(nameof(ApparatusData.Apparatus))]
    public ICollection<ApparatusData> Data { get; set; }
}
public class ApparatusData
{
    public long Id { get; set; }
    public double Raw { get; set; }
    public DateTime Time { get; set; }

    public DeviceApparatus Apparatus { get; set; }
}
public class DeviceConfig
{
    public int Id { get; set; }
    public string Data { get; set; }
}
