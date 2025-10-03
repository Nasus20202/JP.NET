using System.Text.Json.Serialization;

namespace ZtmBus.Models;

public class StopInfo
{
    [JsonPropertyName("stopId")]
    public int StopId { get; set; }

    [JsonPropertyName("stopCode")]
    public string? StopCode { get; set; }

    [JsonPropertyName("stopName")]
    public string? StopName { get; set; }

    [JsonPropertyName("stopShortName")]
    public string? StopShortName { get; set; }

    [JsonPropertyName("stopDesc")]
    public string StopDesc { get; set; } = "";

    [JsonPropertyName("subName")]
    public string? SubName { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("zoneId")]
    public int? ZoneId { get; set; }

    [JsonPropertyName("stopLat")]
    public double StopLat { get; set; }

    [JsonPropertyName("stopLon")]
    public double StopLon { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("zoneName")]
    public string ZoneName { get; set; } = "";

    [JsonPropertyName("wheelchairBoarding")]
    public int? WheelchairBoarding { get; set; }

    [JsonPropertyName("virtual")]
    public int? Virtual { get; set; }

    [JsonPropertyName("nonpassenger")]
    public int? NonPassenger { get; set; }

    [JsonPropertyName("depot")]
    public int? Depot { get; set; }

    [JsonPropertyName("activationDate")]
    public string? ActivationDate { get; set; }
}

public class DepartureInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("delayInSeconds")]
    public int? DelayInSeconds { get; set; }

    [JsonPropertyName("estimatedTime")]
    public DateTime EstimatedTime { get; set; }

    [JsonPropertyName("headsign")]
    public string Headsign { get; set; } = "";

    [JsonPropertyName("routeShortName")]
    public string RouteShortName { get; set; } = "";

    [JsonPropertyName("routeId")]
    public int RouteId { get; set; }

    [JsonPropertyName("scheduledTripStartTime")]
    public DateTime ScheduledTripStartTime { get; set; }

    [JsonPropertyName("tripId")]
    public int TripId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("theoreticalTime")]
    public DateTime TheoreticalTime { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("trip")]
    public int Trip { get; set; }

    [JsonPropertyName("vehicleCode")]
    public int? VehicleCode { get; set; }

    [JsonPropertyName("vehicleId")]
    public int? VehicleId { get; set; }

    [JsonPropertyName("vehicleService")]
    public string? VehicleService { get; set; }
}

public class DeparturesResponse
{
    [JsonPropertyName("lastUpdate")]
    public DateTime LastUpdate { get; set; }

    [JsonPropertyName("departures")]
    public List<DepartureInfo> Departures { get; set; } = new();
}

public class StopsResponse
{
    public Dictionary<string, StopsData> Data { get; set; } = new();
}

public class StopsData
{
    [JsonPropertyName("lastUpdate")]
    public string LastUpdate { get; set; } = "";

    [JsonPropertyName("stops")]
    public List<StopInfo> Stops { get; set; } = new();
}
