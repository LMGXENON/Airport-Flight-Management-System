using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AFMS.Models;
using AFMS.Services;
using System.Globalization;
using Microsoft.Extensions.Caching.Memory;

namespace AFMS.Controllers;

public class HomeController : Controller
{
    private readonly AeroDataBoxService _aeroDataBoxService;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    public HomeController(AeroDataBoxService aeroDataBoxService, IConfiguration configuration, IMemoryCache cache)
    {
        _aeroDataBoxService = aeroDataBoxService;
        _configuration = configuration;
        _cache = cache;
    }

    public async Task<IActionResult> Index()
    {
        var airportCode = _configuration["AeroDataBox:DefaultAirport"] ?? "EGLL"; // London Heathrow ICAO
        
        // Get London local time (GMT/BST)
        var londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        var londonTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, londonTimeZone);
        
        // Use cache to avoid hitting API rate limits (BASIC plan has strict limits)
        var cacheKey = $"flights_{airportCode}_{londonTime:yyyyMMddHH}";
        
        var flights = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            // Cache for 2 minutes to reduce API calls
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
            return await _aeroDataBoxService.GetAirportFlightsAsync(airportCode, londonTime);
        });
        
        // Sort flights by the LHR leg time (departure time for departures, arrival time for arrivals)
        var sortedFlights = (flights ?? new List<AeroDataBoxFlight>())
            .OrderBy(f => {
                var leg = f.Direction == "Departure" ? f.Departure : f.Arrival;
                return ParseLocalDate(leg?.ScheduledTime?.Local) ?? DateTime.MaxValue;
            })
            .ThenBy(f => f.Number)
            .ToList();
        
        return View(sortedFlights);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public async Task<IActionResult> AdvancedSearch(
        string? search,
        string? flight,
        string? airline,
        string? destination,
        DateTime? departureDate,
        DateTime? arrivalDate,
        string? terminal,
        string? status)
    {
        var model = new AdvancedSearchViewModel
        {
            Flight = flight,
            Airline = airline,
            Destination = destination,
            DepartureDate = departureDate,
            ArrivalDate = arrivalDate,
            Terminal = terminal,
            Status = status,
            HasSearched = !string.IsNullOrWhiteSpace(search)
        };

        if (!model.HasSearched)
        {
            return View(model);
        }

        var defaultAirport = (_configuration["AeroDataBox:DefaultAirport"] ?? "EGLL").Trim().ToUpperInvariant();
        var airportCode = defaultAirport;

        var londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        var londonNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, londonTimeZone);

        DateTime from;
        DateTime to;

        if (model.DepartureDate.HasValue || model.ArrivalDate.HasValue)
        {
            var minDate = new[] { model.DepartureDate, model.ArrivalDate }
                .Where(d => d.HasValue)
                .Min()!.Value.Date;

            var maxDate = new[] { model.DepartureDate, model.ArrivalDate }
                .Where(d => d.HasValue)
                .Max()!.Value.Date;

            from = minDate;
            to = maxDate.AddDays(1).AddMinutes(-1);
        }
        else
        {
            // When no dates specified, search current day only to avoid multi-day API issues
            // Start from beginning of today or 6 hours ago, whichever is later
            var todayStart = londonNow.Date;
            var sixHoursAgo = londonNow.AddHours(-6);
            from = sixHoursAgo > todayStart ? sixHoursAgo : todayStart;
            
            // End at end of today (23:59:59)
            to = londonNow.Date.AddDays(1).AddSeconds(-1);
        }

        if (to < from)
        {
            to = from.AddHours(24);
        }

        // Note: Removed 48-hour limit since we're now staying within single day by default

        var flights = await _aeroDataBoxService.GetAirportFlightsAsync(airportCode, from, to, withCancelled: true);
        
        Console.WriteLine($"[DEBUG] Total flights from API: {flights.Count}");
        Console.WriteLine($"[DEBUG] Search filters - Flight: '{model.Flight}', Airline: '{model.Airline}', Destination: '{model.Destination}', Terminal: '{model.Terminal}'");
        Console.WriteLine($"[DEBUG] Date range: {from} to {to}");
        
        foreach (var f in flights.Take(3))
        {
            Console.WriteLine($"[DEBUG] Sample flight: {f.Number}, Airline: {f.Airline?.Name}, Direction: {f.Direction}");
        }
        
        model.Results = ApplyFilters(flights, model)
            .OrderBy(f => ParseLocalDate(f.Departure?.ScheduledTime?.Local) ?? DateTime.MaxValue)
            .ThenBy(f => f.Number)
            .ToList();

        Console.WriteLine($"[DEBUG] Filtered results: {model.Results.Count}");
        
        model.UsedAirportCode = airportCode;
        return View(model);
    }

    private static IEnumerable<AeroDataBoxFlight> ApplyFilters(IEnumerable<AeroDataBoxFlight> flights, AdvancedSearchViewModel model)
    {
        var query = flights;

        if (!string.IsNullOrWhiteSpace(model.Flight))
        {
            var flightValue = model.Flight.Trim();
            Console.WriteLine($"[DEBUG] Filtering by Flight: '{flightValue}'");
            query = query.Where(f => (f.Number ?? string.Empty).Contains(flightValue, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(model.Airline))
        {
            var airline = model.Airline.Trim();
            Console.WriteLine($"[DEBUG] Filtering by Airline: '{airline}'");
            var beforeCount = query.Count();
            query = query.Where(f => (f.Airline?.Name ?? string.Empty).Contains(airline, StringComparison.OrdinalIgnoreCase));
            var afterCount = query.Count();
            Console.WriteLine($"[DEBUG] Airline filter reduced from {beforeCount} to {afterCount} flights");
        }

        if (!string.IsNullOrWhiteSpace(model.Destination))
        {
            var destination = model.Destination.Trim();
            query = query.Where(f => AirportMatches(f.Arrival?.Airport, destination));
        }

        if (!string.IsNullOrWhiteSpace(model.Terminal))
        {
            var terminal = model.Terminal.Trim();
            query = query.Where(f =>
                (f.Departure?.Terminal ?? string.Empty).Contains(terminal, StringComparison.OrdinalIgnoreCase)
                || (f.Arrival?.Terminal ?? string.Empty).Contains(terminal, StringComparison.OrdinalIgnoreCase));
        }

        if (model.DepartureDate.HasValue)
        {
            var depDate = model.DepartureDate.Value.Date;
            query = query.Where(f =>
            {
                var value = ParseLocalDate(f.Departure?.ScheduledTime?.Local);
                return value.HasValue && value.Value.Date == depDate;
            });
        }

        if (model.ArrivalDate.HasValue)
        {
            var arrDate = model.ArrivalDate.Value.Date;
            query = query.Where(f =>
            {
                var value = ParseLocalDate(f.Arrival?.ScheduledTime?.Local);
                return value.HasValue && value.Value.Date == arrDate;
            });
        }

        if (!string.IsNullOrWhiteSpace(model.Status))
        {
            var status = model.Status.Trim().ToLower();
            query = query.Where(f => (f.Status ?? string.Empty).Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        return query;
    }

    private static bool AirportMatches(Airport? airport, string codeOrName)
    {
        if (airport == null)
        {
            return false;
        }

        return (airport.Iata ?? string.Empty).Equals(codeOrName, StringComparison.OrdinalIgnoreCase)
               || (airport.Icao ?? string.Empty).Equals(codeOrName, StringComparison.OrdinalIgnoreCase)
               || (airport.Name ?? string.Empty).Contains(codeOrName, StringComparison.OrdinalIgnoreCase);
    }

    private static DateTime? ParseLocalDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed)
            ? parsed
            : null;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}