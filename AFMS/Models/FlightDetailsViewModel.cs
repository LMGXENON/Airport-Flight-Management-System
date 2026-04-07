using AFMS.Models;
using AFMS.Services;

namespace AFMS.Models;

/// <summary>
/// View model for displaying flight details.
/// Encapsulates all formatted data needed for the Details view.
/// </summary>
public class FlightDetailsViewModel
{
    public int Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string Airline { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    
    // Formatted times
    public string DepartureTimeFormatted { get; set; } = string.Empty;
    public string ArrivalTimeFormatted { get; set; } = string.Empty;
    
    // Raw values for calculations
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    
    // Display values
    public string AircraftType { get; set; } = "TBD";
    public string FormattedGate { get; set; } = string.Empty;
    public string FormattedTerminal { get; set; } = string.Empty;
    public string FlightDuration { get; set; } = string.Empty;
    
    // Status information
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
    
    // Metadata
    public bool IsManualEntry { get; set; }

    /// <summary>
    /// Creates a ViewModel from a Flight model using the FlightDetailsService.
    /// </summary>
    public static FlightDetailsViewModel FromFlight(Flight flight, FlightDetailsService detailsService)
    {
        var (hours, minutes) = detailsService.GetFlightDuration(flight.DepartureTime, flight.ArrivalTime);
        
        return new FlightDetailsViewModel
        {
            Id = flight.Id,
            FlightNumber = flight.FlightNumber,
            Airline = flight.Airline,
            Origin = NormalizeAirportForDisplay(flight.Origin),
            Destination = NormalizeAirportForDisplay(flight.Destination),
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            DepartureTimeFormatted = detailsService.FormatDateAndTime(flight.DepartureTime),
            ArrivalTimeFormatted = detailsService.FormatDateAndTime(flight.ArrivalTime),
            AircraftType = detailsService.GetDisplayValue(flight.AircraftType),
            FormattedGate = detailsService.FormatGate(flight.Gate),
            FormattedTerminal = detailsService.FormatTerminal(flight.Terminal),
            FlightDuration = $"{hours}h {minutes}m",
            Status = flight.Status,
            StatusLabel = detailsService.GetStatusLabel(flight.Status),
            StatusClass = detailsService.GetStatusClass(flight.Status),
            IsManualEntry = flight.IsManualEntry
        };
    }

    private static string NormalizeAirportForDisplay(string? airportCode)
    {
        if (string.IsNullOrWhiteSpace(airportCode))
            return string.Empty;

        var normalized = FlightFormattingHelpers.ConvertToIata(airportCode);
        return string.IsNullOrWhiteSpace(normalized) ? airportCode.Trim() : normalized;
    }
}
