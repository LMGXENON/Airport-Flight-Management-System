using AFMS.Models;

namespace AFMS.Services;

/// <summary>
/// Centralizes manual-flight overlay logic so dashboard and advanced search
/// produce consistent flight rows.
/// </summary>
public class ManualFlightMergeService
{
    public List<AeroDataBoxFlight> MergeManualFlights(
        IEnumerable<AeroDataBoxFlight> apiFlights,
        IEnumerable<Flight> manualFlights)
    {
        var mergedFlights = apiFlights.ToList();

        foreach (var manualFlight in manualFlights.Where(f => f.IsManualEntry))
        {
            var existing = mergedFlights.FirstOrDefault(f =>
                string.Equals(f.Number?.Trim(), manualFlight.FlightNumber.Trim(), StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                var lhrLeg = existing.Direction == "Departure" ? existing.Departure : existing.Arrival;
                if (lhrLeg != null)
                {
                    if (!string.IsNullOrWhiteSpace(manualFlight.Gate))
                        lhrLeg.Gate = manualFlight.Gate;
                    if (!string.IsNullOrWhiteSpace(manualFlight.Terminal))
                        lhrLeg.Terminal = manualFlight.Terminal;
                    if (!string.IsNullOrWhiteSpace(manualFlight.Status))
                        lhrLeg.Status = manualFlight.Status;
                }

                if (!string.IsNullOrWhiteSpace(manualFlight.Status))
                    existing.Status = manualFlight.Status;

                continue;
            }

            mergedFlights.Add(CreateSyntheticFlight(manualFlight));
        }

        return mergedFlights;
    }

    private static AeroDataBoxFlight CreateSyntheticFlight(Flight flight) => new()
    {
        Number = flight.FlightNumber,
        Status = flight.Status,
        Direction = "Departure",
        Airline = new Airline { Name = flight.Airline },
        Departure = new FlightMovement
        {
            Airport = new Airport { Iata = "LHR", Icao = "EGLL", Name = "London Heathrow" },
            Gate = flight.Gate,
            Terminal = flight.Terminal,
            Status = flight.Status,
            ScheduledTime = new ScheduledTime
            {
                Local = flight.DepartureTime.ToString("yyyy-MM-ddTHH:mmzzz"),
                Utc = flight.DepartureTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mmZ")
            }
        },
        Arrival = new FlightMovement
        {
            Airport = new Airport { Iata = flight.Destination, Name = flight.Destination },
            ScheduledTime = new ScheduledTime
            {
                Local = flight.ArrivalTime.ToString("yyyy-MM-ddTHH:mmzzz"),
                Utc = flight.ArrivalTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mmZ")
            }
        }
    };
}