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
        var flightsByNumber = mergedFlights
            .Select(flight => new { Flight = flight, Key = NormalizeFlightNumberKey(flight.Number) })
            .Where(item => item.Key != null)
            .GroupBy(item => item.Key!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().Flight, StringComparer.OrdinalIgnoreCase);

        foreach (var manualFlight in manualFlights.Where(f => f.IsManualEntry))
        {
            var normalizedStatus = string.IsNullOrWhiteSpace(manualFlight.Status)
                ? null
                : FlightStatusCatalog.Normalize(manualFlight.Status);
            var flightNumberKey = NormalizeFlightNumberKey(manualFlight.FlightNumber);

            if (flightNumberKey == null)
            {
                mergedFlights.Add(CreateSyntheticFlight(manualFlight));
                continue;
            }

            flightsByNumber.TryGetValue(flightNumberKey, out var existing);

            if (existing != null)
            {
                var lhrLeg = existing.Direction == "Departure" ? existing.Departure : existing.Arrival;
                if (lhrLeg != null)
                {
                    if (!string.IsNullOrWhiteSpace(manualFlight.Gate))
                        lhrLeg.Gate = manualFlight.Gate;
                    if (!string.IsNullOrWhiteSpace(manualFlight.Terminal))
                        lhrLeg.Terminal = manualFlight.Terminal;
                    if (!string.IsNullOrWhiteSpace(normalizedStatus))
                        lhrLeg.Status = normalizedStatus;
                }

                if (!string.IsNullOrWhiteSpace(normalizedStatus))
                    existing.Status = normalizedStatus;

                if (!string.IsNullOrWhiteSpace(manualFlight.AircraftType))
                {
                    existing.Aircraft ??= new Aircraft();
                    existing.Aircraft.Model = manualFlight.AircraftType;
                }

                continue;
            }

            var syntheticFlight = CreateSyntheticFlight(manualFlight);
            mergedFlights.Add(syntheticFlight);

            var syntheticKey = NormalizeFlightNumberKey(syntheticFlight.Number);
            if (syntheticKey != null && !flightsByNumber.ContainsKey(syntheticKey))
            {
                flightsByNumber[syntheticKey] = syntheticFlight;
            }
        }

        return mergedFlights;
    }

    private static string? NormalizeFlightNumberKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim().Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
    }

    private static AeroDataBoxFlight CreateSyntheticFlight(Flight flight)
    {
        var normalizedStatus = FlightStatusCatalog.Normalize(flight.Status);

        return new AeroDataBoxFlight
        {
            Number = flight.FlightNumber,
            Status = normalizedStatus,
            Direction = "Departure",
            Airline = new Airline { Name = flight.Airline },
            Aircraft = string.IsNullOrWhiteSpace(flight.AircraftType)
                ? null
                : new Aircraft { Model = flight.AircraftType },
            Departure = new FlightMovement
            {
                Airport = new Airport { Iata = "LHR", Icao = "EGLL", Name = "London Heathrow" },
                Gate = flight.Gate,
                Terminal = flight.Terminal,
                Status = normalizedStatus,
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
}