using System.Text.Json;
using System.Text.Json.Serialization;

namespace web_ui.Data;

public class ApiApparatusData
{
    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("raw")]
    public double Raw { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("mean")]
    public double Mean { get; set; }

    [JsonPropertyName("stDev")]
    public double StDev { get; set; }

    [JsonPropertyName("valid")]
    public int Valid { get; set; }

    [JsonPropertyName("values")]
    public List<double> Values { get; set; }
}


public class ApiDeviceSpecs
{
    [JsonPropertyName("chipId")]
    public string ChipId { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }
}
public class ApiDeviceRoutes
{
    [JsonPropertyName("route")]
    public string Route { get; set; }

    [JsonPropertyName("devices")]
    public List<ApiRouteInfo> Devices { get; set; }


}

public class ApiRouteInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }
}
