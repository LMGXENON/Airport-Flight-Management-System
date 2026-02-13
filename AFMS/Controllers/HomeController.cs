using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AFMS.Models;
using AFMS.Services;
using System.Globalization;

namespace AFMS.Controllers;

public class HomeController : Controller
{
    private readonly AeroDataBoxService _aeroDataBoxService;
    private readonly IConfiguration _configuration;

    public HomeController(AeroDataBoxService aeroDataBoxService, IConfiguration configuration)
    {
        _aeroDataBoxService = aeroDataBoxService;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        var airportCode = _configuration["AeroDataBox:DefaultAirport"] ?? "EGLL"; // London Heathrow ICAO
        
        // Get London local time (GMT/BST)
        var londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        var londonTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, londonTimeZone);
        
        var flights = await _aeroDataBoxService.GetAirportFlightsAsync(airportCode, londonTime);
        return View(flights);
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
        string? terminal)
    {
        var model = new AdvancedSearchViewModel
        {
            Flight = flight,
            Airline = airline,
            Destination = destination,
            DepartureDate = departureDate,
            ArrivalDate = arrivalDate,
            Terminal = terminal,
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
            // Keep results broad when dates are missing.
            from = londonNow.AddHours(-6);
            to = londonNow.AddHours(24);
        }

        if (to < from)
        {
            to = from.AddHours(24);
        }

        if ((to - from).TotalHours > 48)
        {
            to = from.AddHours(48);
            model.Notice = "Search window limited to 48 hours for API performance.";
        }

        var flights = await _aeroDataBoxService.GetAirportFlightsAsync(airportCode, from, to, withCancelled: true);
        model.Results = ApplyFilters(flights, model)
            .OrderBy(f => ParseLocalDate(f.Departure?.ScheduledTime?.Local) ?? DateTime.MaxValue)
            .ThenBy(f => f.Number)
            .ToList();

        model.UsedAirportCode = airportCode;
        return View(model);
    }

    private static IEnumerable<AeroDataBoxFlight> ApplyFilters(IEnumerable<AeroDataBoxFlight> flights, AdvancedSearchViewModel model)
    {
        var query = flights;

        if (!string.IsNullOrWhiteSpace(model.Flight))
        {
            var flightValue = model.Flight.Trim();
            query = query.Where(f => (f.Number ?? string.Empty).Contains(flightValue, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(model.Airline))
        {
            var airline = model.Airline.Trim();
            query = query.Where(f => (f.Airline?.Name ?? string.Empty).Contains(airline, StringComparison.OrdinalIgnoreCase));
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