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
            var manualGate = Clean(manualFlight.Gate);
            var manualTerminal = Clean(manualFlight.Terminal);
            var manualAircraftType = Clean(manualFlight.AircraftType);
            var hasManualGate = !string.IsNullOrWhiteSpace(manualGate);
            var hasManualTerminal = !string.IsNullOrWhiteSpace(manualTerminal);
            var hasManualAircraftType = !string.IsNullOrWhiteSpace(manualAircraftType);
            var normalizedStatus = NormalizeManualStatus(manualFlight.Status);
            var hasManualStatus = !string.IsNullOrWhiteSpace(normalizedStatus);
            var flightNumberKey = NormalizeFlightNumberKey(manualFlight.FlightNumber);

            if (flightNumberKey == null)
            {
                mergedFlights.Add(CreateSyntheticFlight(manualFlight));
                continue;
            }

            flightsByNumber.TryGetValue(flightNumberKey, out var existing);

            if (existing != null)
            {
                var direction = existing.Direction?.Trim();
                var lhrLeg = string.Equals(direction, "Arrival", StringComparison.OrdinalIgnoreCase)
                    ? existing.Arrival
                    : string.Equals(direction, "Departure", StringComparison.OrdinalIgnoreCase)
                        ? existing.Departure
                        : existing.Departure ?? existing.Arrival;

                if (lhrLeg == null)
                {
                    lhrLeg = new FlightMovement();
                    if (string.Equals(direction, "Arrival", StringComparison.OrdinalIgnoreCase))
                        existing.Arrival = lhrLeg;
                    else
                        existing.Departure = lhrLeg;
                }

                if (lhrLeg != null)
                {
                    if (hasManualGate)
                        lhrLeg.Gate = manualGate;
                    if (hasManualTerminal)
                        lhrLeg.Terminal = manualTerminal;
                    if (hasManualStatus)
                        lhrLeg.Status = normalizedStatus;
                }

                if (hasManualStatus)
                    existing.Status = normalizedStatus;

                if (hasManualAircraftType)
                {
                    existing.Aircraft ??= new Aircraft();
                    existing.Aircraft.Model = manualAircraftType;
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

        var normalized = new string(value.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        return normalized.Length == 0 ? null : normalized;
    }

    private static AeroDataBoxFlight CreateSyntheticFlight(Flight flight)
    {
        var flightNumber = Clean(flight.FlightNumber);
        var airline = Clean(flight.Airline);
        var aircraftType = Clean(flight.AircraftType);
        var gate = Clean(flight.Gate);
        var terminal = Clean(flight.Terminal);
        var destination = Clean(flight.Destination);
        var normalizedStatus = FlightStatusCatalog.Normalize(flight.Status);
        var departureUtc = DateTime.SpecifyKind(flight.DepartureTime, DateTimeKind.Utc);
        var arrivalUtc = DateTime.SpecifyKind(flight.ArrivalTime, DateTimeKind.Utc);

        return new AeroDataBoxFlight
        {
            Number = flightNumber,
            Status = normalizedStatus,
            Direction = "Departure",
            Airline = string.IsNullOrWhiteSpace(airline)
                ? null
                : new Airline { Name = airline },
            Aircraft = string.IsNullOrWhiteSpace(aircraftType)
                ? null
                : new Aircraft { Model = aircraftType },
            Departure = new FlightMovement
            {
                Airport = new Airport { Iata = "LHR", Icao = "EGLL", Name = "London Heathrow" },
                Gate = gate,
                Terminal = terminal,
                Status = normalizedStatus,
                ScheduledTime = new ScheduledTime
                {
                    Local = departureUtc.ToLocalTime().ToString("yyyy-MM-ddTHH:mmzzz"),
                    Utc = departureUtc.ToString("yyyy-MM-ddTHH:mm:ssZ")
                }
            },
            Arrival = new FlightMovement
            {
                Airport = new Airport { Iata = destination, Name = destination },
                ScheduledTime = new ScheduledTime
                {
                    Local = arrivalUtc.ToLocalTime().ToString("yyyy-MM-ddTHH:mmzzz"),
                    Utc = arrivalUtc.ToString("yyyy-MM-ddTHH:mm:ssZ")
                }
            }
        };
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeManualStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        return FlightStatusCatalog.IsKnown(status)
            ? FlightStatusCatalog.Normalize(status)
            : null;
    }
}