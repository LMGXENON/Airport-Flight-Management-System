namespace AFMS.Models;

public class AeroDataBoxFlight
{
    public string? Number { get; set; }
    public string? CallSign { get; set; }
    public FlightMovement? Departure { get; set; }
    public FlightMovement? Arrival { get; set; }
    public Airline? Airline { get; set; }
    public Aircraft? Aircraft { get; set; }
    public string? Status { get; set; }
    public string? CodeshareStatus { get; set; }
    public bool IsCargo { get; set; }

    /// <summary>Populated after deserialization: "Departure" or "Arrival" relative to the home airport.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string Direction { get; set; } = "";
}

public class FlightMovement
{
    public Airport? Airport { get; set; }
    public ScheduledTime? ScheduledTime { get; set; }
    public ScheduledTime? RevisedTime { get; set; }
    public ScheduledTime? PredictedTime { get; set; }
    public ScheduledTime? RunwayTime { get; set; }
    public string? Terminal { get; set; }
    public string? Gate { get; set; }
    public string? CheckInDesk { get; set; }
    public string? BaggageBelt { get; set; }
    public string? Runway { get; set; }
    public List<string>? Quality { get; set; }
}

public class Airport
{
    public string? Icao { get; set; }
    public string? Iata { get; set; }
    public string? Name { get; set; }
    public string? ShortName { get; set; }
    public string? MunicipalityName { get; set; }
    public string? CountryCode { get; set; }
}

public class ScheduledTime
{
    public string? Local { get; set; }
    public string? Utc { get; set; }
}

public class Airline
{
    public string? Name { get; set; }
    public string? Iata { get; set; }
    public string? Icao { get; set; }
}

public class Aircraft
{
    public string? Model { get; set; }
    public string? Reg { get; set; }
    public string? ModeS { get; set; }
}

public class AeroDataBoxResponse
{
    public List<AeroDataBoxFlight>? Departures { get; set; }
    public List<AeroDataBoxFlight>? Arrivals { get; set; }
}
