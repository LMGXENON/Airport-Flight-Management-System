using AFMS.Models;

namespace AFMS.Services;

/// <summary>
/// Encapsulates all logic for the Advanced Search feature:
/// API dispatch, in-memory filtering, sorting, and pagination.
/// </summary>
public class FlightSearchService
{
    private readonly AeroDataBoxService _aeroDataBoxService;
    private readonly IConfiguration _configuration;

    public FlightSearchService(AeroDataBoxService aeroDataBoxService, IConfiguration configuration)
    {
        _aeroDataBoxService = aeroDataBoxService;
        _configuration = configuration;
    }

    /// <summary>
    /// Fetches flights from the API, applies all filters/sort/pagination from the model,
    /// and writes the results back into <paramref name="model"/>.
    /// </summary>
    public async Task ExecuteSearchAsync(AdvancedSearchViewModel model)
    {
        var airportCode = (_configuration["AeroDataBox:DefaultAirport"] ?? "EGLL").Trim().ToUpperInvariant();
        var londonTz   = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        var londonNow  = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, londonTz);

        // Determine the API date window
        DateTime from, to;

        if (model.DepartureDate.HasValue || model.ArrivalDate.HasValue)
        {
            var minDate = new[] { model.DepartureDate, model.ArrivalDate }
                .Where(d => d.HasValue).Select(d => d!.Value.Date).Min();
            var maxDate = new[] { model.DepartureDate, model.ArrivalDate }
                .Where(d => d.HasValue).Select(d => d!.Value.Date).Max();

            from = minDate;
            to   = maxDate.AddDays(1).AddMinutes(-1);
        }
        else
        {
            from = londonNow;
            to   = londonNow.AddHours(12);
        }

        if (to < from) to = from.AddHours(24);

        var allFlights = await _aeroDataBoxService.GetAirportFlightsAsync(airportCode, from, to, withCancelled: true);

        var filtered = ApplyFilters(allFlights, model);
        var sorted   = ApplySorting(filtered, model);

        model.UsedAirportCode = airportCode;
        model.TotalCount      = sorted.Count;

        // Paginate
        model.Results = sorted
            .Skip((model.Page - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToList();
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static List<AeroDataBoxFlight> ApplyFilters(IEnumerable<AeroDataBoxFlight> flights, AdvancedSearchViewModel model)
    {
        var query = flights;

        // Direction
        if (!string.IsNullOrWhiteSpace(model.Direction))
            query = query.Where(f => f.Direction.Equals(model.Direction, StringComparison.OrdinalIgnoreCase));

        // Flight number
        if (!string.IsNullOrWhiteSpace(model.Flight))
        {
            var raw        = model.Flight.Trim();
            var normalized = Normalize(raw);
            query = query.Where(f =>
                (f.Number ?? "").Contains(raw, StringComparison.OrdinalIgnoreCase)
                || Normalize(f.Number).Contains(normalized, StringComparison.OrdinalIgnoreCase));
        }

        // Airline
        if (!string.IsNullOrWhiteSpace(model.Airline))
            query = query.Where(f => (f.Airline?.Name ?? "").Contains(model.Airline.Trim(), StringComparison.OrdinalIgnoreCase));

        // Destination airport
        if (!string.IsNullOrWhiteSpace(model.Destination))
            query = query.Where(f => AirportMatches(f.Arrival?.Airport, model.Destination.Trim()));

        // Terminal
        if (!string.IsNullOrWhiteSpace(model.Terminal))
        {
            var t = model.Terminal.Trim();
            query = query.Where(f =>
                (f.Departure?.Terminal ?? "").Contains(t, StringComparison.OrdinalIgnoreCase)
                || (f.Arrival?.Terminal ?? "").Contains(t, StringComparison.OrdinalIgnoreCase));
        }

        // Departure date
        if (model.DepartureDate.HasValue)
        {
            var d = model.DepartureDate.Value.Date;
            query = query.Where(f =>
            {
                var v = AdvancedSearchViewModel.ParseLocalDate(f.Departure?.ScheduledTime?.Local);
                return v.HasValue && v.Value.Date == d;
            });
        }

        // Arrival date
        if (model.ArrivalDate.HasValue)
        {
            var d = model.ArrivalDate.Value.Date;
            query = query.Where(f =>
            {
                var v = AdvancedSearchViewModel.ParseLocalDate(f.Arrival?.ScheduledTime?.Local);
                return v.HasValue && v.Value.Date == d;
            });
        }

        // Multi-select status (Statuses list takes priority; falls back to single Status)
        var activeStatuses = model.Statuses.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        if (!activeStatuses.Any() && !string.IsNullOrWhiteSpace(model.Status))
            activeStatuses.Add(model.Status.Trim());

        if (activeStatuses.Any())
        {
            query = query.Where(f =>
            {
                var isDep          = f.Direction == "Departure";
                var movementStatus = isDep ? f.Departure?.Status : f.Arrival?.Status;
                var flightStatus   = (movementStatus ?? f.Status ?? "").ToLower();
                return activeStatuses.Any(s => flightStatus.Equals(s, StringComparison.OrdinalIgnoreCase));
            });
        }

        // Time range – applied to the "home leg" scheduled time
        if (!string.IsNullOrWhiteSpace(model.TimeRangeStart) && TimeSpan.TryParse(model.TimeRangeStart, out var tStart))
        {
            query = query.Where(f =>
            {
                var leg = f.Direction == "Departure" ? f.Departure : f.Arrival;
                var dt  = AdvancedSearchViewModel.ParseLocalDate(leg?.ScheduledTime?.Local);
                return dt.HasValue && dt.Value.TimeOfDay >= tStart;
            });
        }

        if (!string.IsNullOrWhiteSpace(model.TimeRangeEnd) && TimeSpan.TryParse(model.TimeRangeEnd, out var tEnd))
        {
            query = query.Where(f =>
            {
                var leg = f.Direction == "Departure" ? f.Departure : f.Arrival;
                var dt  = AdvancedSearchViewModel.ParseLocalDate(leg?.ScheduledTime?.Local);
                return dt.HasValue && dt.Value.TimeOfDay <= tEnd;
            });
        }

        return query.ToList();
    }

    private static List<AeroDataBoxFlight> ApplySorting(IEnumerable<AeroDataBoxFlight> flights, AdvancedSearchViewModel model)
    {
        bool desc = model.SortOrder?.ToLower() == "desc";

        IOrderedEnumerable<AeroDataBoxFlight> ordered = model.SortBy?.ToLower() switch
        {
            "flight"  => desc ? flights.OrderByDescending(f => f.Number)       : flights.OrderBy(f => f.Number),
            "airline" => desc ? flights.OrderByDescending(f => f.Airline?.Name) : flights.OrderBy(f => f.Airline?.Name),
            "arrival" => desc
                ? flights.OrderByDescending(f => AdvancedSearchViewModel.ParseLocalDate(f.Arrival?.ScheduledTime?.Local) ?? DateTime.MaxValue)
                : flights.OrderBy(f => AdvancedSearchViewModel.ParseLocalDate(f.Arrival?.ScheduledTime?.Local) ?? DateTime.MaxValue),
            "status"  => desc ? flights.OrderByDescending(f => f.Status) : flights.OrderBy(f => f.Status),
            _ => desc  // default: sort by departure time
                ? flights.OrderByDescending(f => AdvancedSearchViewModel.ParseLocalDate(f.Departure?.ScheduledTime?.Local) ?? DateTime.MaxValue)
                : flights.OrderBy(f => AdvancedSearchViewModel.ParseLocalDate(f.Departure?.ScheduledTime?.Local) ?? DateTime.MaxValue)
        };

        return ordered.ThenBy(f => f.Number).ToList();
    }

    private static bool AirportMatches(Airport? airport, string codeOrName)
    {
        if (airport == null) return false;
        return (airport.Iata ?? "").Equals(codeOrName, StringComparison.OrdinalIgnoreCase)
            || (airport.Icao ?? "").Equals(codeOrName, StringComparison.OrdinalIgnoreCase)
            || (airport.Name ?? "").Contains(codeOrName, StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : new string(value.Where(char.IsLetterOrDigit).ToArray());
}
