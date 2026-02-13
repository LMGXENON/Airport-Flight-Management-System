namespace AFMS.Models;

public class AdvancedSearchViewModel
{
    public string? Flight { get; set; }
    public string? Airline { get; set; }
    public string? Destination { get; set; }
    public DateTime? DepartureDate { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public string? Terminal { get; set; }
    public string? Status { get; set; }
    public bool HasSearched { get; set; }
    public string? Notice { get; set; }
    public string? UsedAirportCode { get; set; }
    public List<AeroDataBoxFlight> Results { get; set; } = new();
}
