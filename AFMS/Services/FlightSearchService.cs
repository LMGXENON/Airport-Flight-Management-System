using AFMS.Models;
using AFMS.Data;
using Microsoft.EntityFrameworkCore;

namespace AFMS.Services;

/// <summary>
/// Encapsulates all logic for the Advanced Search feature:
/// API dispatch, in-memory filtering, sorting, and pagination.
/// </summary>
public class FlightSearchService
{
    private static readonly HashSet<string> AirlineNoiseWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "air", "airline", "airlines", "airway", "airways", "international", "intl", "lines"
    };

    private readonly AeroDataBoxService _aeroDataBoxService;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public FlightSearchService(
        AeroDataBoxService aeroDataBoxService,
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        _aeroDataBoxService = aeroDataBoxService;
        _configuration = configuration;
        _context = context;
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

        var apiFlights = await _aeroDataBoxService.GetAirportFlightsAsync(airportCode, from, to, withCancelled: true);
        var allFlights = await MergeManualFlightsAsync(apiFlights);

        var filtered = ApplyFilters(allFlights, model);
        var sorted   = ApplySorting(filtered, model);

        model.UsedAirportCode = airportCode;
        model.TotalCount      = sorted.Count;

        var totalPages = Math.Max(1, (int)Math.Ceiling(model.TotalCount / (double)model.PageSize));
        model.Page = model.TotalCount == 0
            ? 1
            : Math.Min(Math.Max(model.Page, 1), totalPages);

        // Paginate
        model.Results = sorted
            .Skip((model.Page - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToList();
    }

    private async Task<List<AeroDataBoxFlight>> MergeManualFlightsAsync(List<AeroDataBoxFlight> apiFlights)
    {
        var mergedFlights = apiFlights.ToList();
        var manualFlights = await _context.Flights
            .AsNoTracking()
            .Where(f => f.IsManualEntry)
            .ToListAsync();

        foreach (var manualFlight in manualFlights)
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

        // Airline — matches full name, any word within it, or IATA/ICAO code prefix
        if (!string.IsNullOrWhiteSpace(model.Airline))
        {
            var term = model.Airline.Trim();
            query = query.Where(f => AirlineMatches(f.Airline, term));
        }

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

        // Multi-select status filter
        var activeStatuses = model.Statuses.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

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

        var term = codeOrName.Trim();
        if ((airport.Iata ?? "").Equals(term, StringComparison.OrdinalIgnoreCase)
            || (airport.Icao ?? "").Equals(term, StringComparison.OrdinalIgnoreCase)
            || (airport.Name ?? "").Contains(term, StringComparison.OrdinalIgnoreCase)
            || (airport.ShortName ?? "").Contains(term, StringComparison.OrdinalIgnoreCase)
            || (airport.MunicipalityName ?? "").Contains(term, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var normalizedTerm = Normalize(term);
        var normalizedName = Normalize(airport.Name);
        var normalizedShortName = Normalize(airport.ShortName);
        var normalizedMunicipality = Normalize(airport.MunicipalityName);

        return normalizedName.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase)
            || normalizedShortName.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase)
            || normalizedMunicipality.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase);
    }

    private static bool AirlineMatches(Airline? airline, string term)
    {
        if (airline == null) return false;

        var name = airline.Name ?? string.Empty;
        var iata = airline.Iata ?? string.Empty;
        var icao = airline.Icao ?? string.Empty;

        if (name.Contains(term, StringComparison.OrdinalIgnoreCase)
            || iata.Equals(term, StringComparison.OrdinalIgnoreCase)
            || icao.Equals(term, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var normalizedTerm = Normalize(term);
        var normalizedName = Normalize(name);

        if (normalizedName.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase)
            || iata.Equals(normalizedTerm, StringComparison.OrdinalIgnoreCase)
            || icao.Equals(normalizedTerm, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var queryTokens = GetMeaningfulTokens(term);
        var nameTokens = GetMeaningfulTokens(name);

        if (queryTokens.Count > 0 && nameTokens.Count > 0 && queryTokens.All(queryToken =>
                nameTokens.Any(nameToken => TokensMatch(queryToken, nameToken))))
        {
            return true;
        }

        var airlineInitialism = string.Concat(nameTokens.Where(t => t.Length > 0).Select(t => char.ToUpperInvariant(t[0])));
        return airlineInitialism.Equals(normalizedTerm, StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> GetMeaningfulTokens(string? value) =>
        (value ?? string.Empty)
            .Split(new[] { ' ', '-', '/', '&', '.' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(Normalize)
            .Where(token => !string.IsNullOrWhiteSpace(token) && !AirlineNoiseWords.Contains(token))
            .ToList();

    private static bool TokensMatch(string queryToken, string nameToken)
    {
        if (queryToken.Equals(nameToken, StringComparison.OrdinalIgnoreCase)
            || nameToken.StartsWith(queryToken, StringComparison.OrdinalIgnoreCase)
            || queryToken.StartsWith(nameToken, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return queryToken.Length >= 4
            && nameToken.Length >= 4
            && LevenshteinDistance(queryToken, nameToken) <= 1;
    }

    private static int LevenshteinDistance(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        var costs = new int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++)
            costs[j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            var previousDiagonal = costs[0];
            costs[0] = i;

            for (var j = 1; j <= b.Length; j++)
            {
                var temp = costs[j];
                var substitutionCost = a[i - 1] == b[j - 1] ? 0 : 1;
                costs[j] = Math.Min(
                    Math.Min(costs[j] + 1, costs[j - 1] + 1),
                    previousDiagonal + substitutionCost);
                previousDiagonal = temp;
            }
        }

        return costs[b.Length];
    }

    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : new string(value.Where(char.IsLetterOrDigit).ToArray());
}
