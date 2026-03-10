namespace AFMS.Models;

public class FlightIndexViewModel
{
    public string? Search { get; set; }
    public IReadOnlyList<Flight> Flights { get; set; } = Array.Empty<Flight>();
    public PaginationState Pagination { get; set; } = new();
}