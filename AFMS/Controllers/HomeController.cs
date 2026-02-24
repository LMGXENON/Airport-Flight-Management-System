using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using AFMS.Models;
using AFMS.Services;

namespace AFMS.Controllers;

public class HomeController : Controller
{
    private readonly AeroDataBoxService _aeroDataBoxService;
    private readonly FlightSearchService _flightSearchService;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    public HomeController(
        AeroDataBoxService aeroDataBoxService,
        FlightSearchService flightSearchService,
        IConfiguration configuration,
        IMemoryCache cache)
    {
        _aeroDataBoxService  = aeroDataBoxService;
        _flightSearchService = flightSearchService;
        _configuration       = configuration;
        _cache               = cache;
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
                return AdvancedSearchViewModel.ParseLocalDate(leg?.ScheduledTime?.Local) ?? DateTime.MaxValue;
            })
            .ThenBy(f => f.Number)
            .ToList();
        
        return View(sortedFlights);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>Full-page Advanced Search — renders the shell with any pre-filled results.</summary>
    public async Task<IActionResult> AdvancedSearch(
        string? search,
        string? flight,
        string? airline,
        string? destination,
        DateTime? departureDate,
        DateTime? arrivalDate,
        string? terminal,
        string? direction,
        [FromQuery(Name = "statuses")] List<string>? statuses,
        string? timeRangeStart,
        string? timeRangeEnd,
        string? sortBy,
        string? sortOrder,
        int page = 1)
    {
        var model = BuildSearchModel(
            search, flight, airline, destination,
            departureDate, arrivalDate, terminal, direction,
            statuses, timeRangeStart, timeRangeEnd,
            sortBy, sortOrder, page);

        if (!model.HasSearched)
            return View(model);

        await _flightSearchService.ExecuteSearchAsync(model);
        return View(model);
    }

    /// <summary>AJAX endpoint — returns only the results partial view.</summary>
    [HttpGet]
    public async Task<IActionResult> AdvancedSearchResults(
        string? flight,
        string? airline,
        string? destination,
        DateTime? departureDate,
        DateTime? arrivalDate,
        string? terminal,
        string? direction,
        [FromQuery(Name = "statuses")] List<string>? statuses,
        string? timeRangeStart,
        string? timeRangeEnd,
        string? sortBy,
        string? sortOrder,
        int page = 1)
    {
        var model = BuildSearchModel(
            "1", flight, airline, destination,
            departureDate, arrivalDate, terminal, direction,
            statuses, timeRangeStart, timeRangeEnd,
            sortBy, sortOrder, page);

        await _flightSearchService.ExecuteSearchAsync(model);
        return PartialView("_AdvancedSearchResults", model);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static AdvancedSearchViewModel BuildSearchModel(
        string? search,
        string? flight, string? airline, string? destination,
        DateTime? departureDate, DateTime? arrivalDate,
        string? terminal, string? direction,
        List<string>? statuses,
        string? timeRangeStart, string? timeRangeEnd,
        string? sortBy, string? sortOrder, int page)
    {
        return new AdvancedSearchViewModel
        {
            Flight          = flight,
            Airline         = airline,
            Destination     = destination,
            DepartureDate   = departureDate,
            ArrivalDate     = arrivalDate,
            Terminal        = terminal,
            Direction       = direction,
            Statuses        = statuses ?? new List<string>(),
            TimeRangeStart  = timeRangeStart,
            TimeRangeEnd    = timeRangeEnd,
            SortBy          = sortBy,
            SortOrder       = sortOrder,
            Page            = page < 1 ? 1 : page,
            HasSearched     = !string.IsNullOrWhiteSpace(search)
        };
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}