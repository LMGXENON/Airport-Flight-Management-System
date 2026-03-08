using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using AFMS.Models;
using AFMS.Services;

namespace AFMS.Controllers;

public class HomeController : Controller
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Expected", "Boarding", "Departed", "Arrived", "Delayed", "Canceled"
    };

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

    [HttpPost]
    [Route("Home/ProcessAIQuery")]
    public async Task<IActionResult> ProcessAIQuery([FromBody] AIQueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Query))
            return BadRequest(new { error = "Query is required" });

        try
        {
            var apiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                return StatusCode(500, new { error = "DeepSeek API key not configured" });

            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
                        var systemPrompt = $@"You fill Advanced Search filters for a London Heathrow flight search page.
You are NOT a general chatbot.

If the message is not clearly about searching or filtering flights, return:
{{
    ""isSearchRequest"": false,
    ""message"": ""I can only help with flight searches. Try asking for a flight by airline, destination, flight number, date, time, terminal or status."" 
}}

If the message is about searching flights, return ONLY a valid JSON object with:
    isSearchRequest – true
    message         – optional short clarification, otherwise empty
    flight          – flight number string (e.g. ""BA123"")
    airline         – airline text from the user; if they only give a partial airline fragment, keep the best airline fragment instead of forcing a made-up full name
    destination     – IATA airport code of the non-LHR endpoint when clear (e.g. ""DOH"", ""JFK"", ""DXB"")
    departureDate   – date string YYYY-MM-DD
    arrivalDate     – date string YYYY-MM-DD
    terminal        – terminal string (e.g. ""5"")
    direction       – ""Departure"" | ""Arrival"" | """" (empty = both)
    timeRangeStart  – HH:mm
    timeRangeEnd    – HH:mm
    statuses        – array from [""Expected"",""Boarding"",""Departed"",""Arrived"",""Delayed"",""Canceled""]

Rules:
- Only return JSON. No markdown. No explanation.
- Use destination codes when clear; otherwise leave destination empty.
- If the user says ""today"", use {today} as the date.
- If the user says ""tomorrow"", use the next calendar day from today.
- Direction hints: ""from heathrow"", ""departing"", ""leaving"" => Departure. ""to heathrow"", ""arriving"", ""landing"" => Arrival.
- City/country to IATA when obvious: Doha/Qatar→DOH, New York→JFK, Dubai→DXB, Paris→CDG, Frankfurt→FRA, Tokyo→HND, Amsterdam→AMS, Madrid→MAD, Singapore→SIN, Los Angeles→LAX, Sydney→SYD, Toronto→YYZ, Chicago→ORD, Miami→MIA, Barcelona→BCN, Rome→FCO, Lisbon→LIS, Istanbul→IST, Athens→ATH, Cairo→CAI, Bangkok→BKK, Hong Kong→HKG, Seoul→ICN, Shanghai→PVG, Beijing→PEK, Kuala Lumpur→KUL, Mumbai→BOM, Delhi→DEL, Nairobi→NBO, Cape Town→CPT, Johannesburg→JNB, Sao Paulo→GRU, Buenos Aires→EZE, Mexico City→MEX, Vancouver→YVR, Montreal→YUL, Boston→BOS, Washington→IAD, San Francisco→SFO, Seattle→SEA, Dallas→DFW, Houston→IAH, Atlanta→ATL, Denver→DEN, Orlando→MCO, Las Vegas→LAS, Manchester→MAN, Edinburgh→EDI, Glasgow→GLA, Birmingham→BHX, Bristol→BRS, Dublin→DUB.
- If nothing usable can be extracted for a search, return isSearchRequest false with the short search-only message.";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = request.Query }
                },
                temperature = 0,
                max_tokens = 300
            };

            var response = await client.PostAsJsonAsync(
                "https://api.deepseek.com/v1/chat/completions",
                requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return BadRequest(new { error = $"DeepSeek API error: {response.StatusCode}", details = errorContent });
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var jsonResponse = JsonSerializer.Deserialize<DeepSeekResponse>(jsonContent, opts);
            var content = jsonResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "{}";
            
            content = System.Text.RegularExpressions.Regex.Replace(content, @"```(?:json)?|```", "").Trim();

            var parsedParams = JsonSerializer.Deserialize<AiSearchFiltersResponse>(content, opts) ?? new AiSearchFiltersResponse();
            var normalizedParams = NormalizeAiSearchFilters(parsedParams);
            return Ok(normalizedParams);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

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

    private static AiSearchFiltersResponse NormalizeAiSearchFilters(AiSearchFiltersResponse response)
    {
        response.Flight = Clean(response.Flight)?.ToUpperInvariant();
        response.Airline = Clean(response.Airline);
        response.Destination = NormalizeDestination(response.Destination);
        response.DepartureDate = NormalizeDate(response.DepartureDate);
        response.ArrivalDate = NormalizeDate(response.ArrivalDate);
        response.Terminal = NormalizeTerminal(response.Terminal);
        response.Direction = NormalizeDirection(response.Direction);
        response.TimeRangeStart = NormalizeTime(response.TimeRangeStart);
        response.TimeRangeEnd = NormalizeTime(response.TimeRangeEnd);
        response.Statuses = (response.Statuses ?? new List<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s) && AllowedStatuses.Contains(s))
            .Select(s => AllowedStatuses.First(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        response.Message = Clean(response.Message);

        if (!response.IsSearchRequest && !HasAnySearchFilters(response))
        {
            response.Message ??= "I can only help with flight searches. Try asking for a flight by airline, destination, flight number, date, time, terminal or status.";
            return response;
        }

        response.IsSearchRequest = HasAnySearchFilters(response);

        if (!response.IsSearchRequest)
        {
            response.Message = "I can only help with flight searches. Try asking for a flight by airline, destination, flight number, date, time, terminal or status.";
        }

        return response;
    }

    private static bool HasAnySearchFilters(AiSearchFiltersResponse response) =>
        !string.IsNullOrWhiteSpace(response.Flight)
        || !string.IsNullOrWhiteSpace(response.Airline)
        || !string.IsNullOrWhiteSpace(response.Destination)
        || !string.IsNullOrWhiteSpace(response.DepartureDate)
        || !string.IsNullOrWhiteSpace(response.ArrivalDate)
        || !string.IsNullOrWhiteSpace(response.Terminal)
        || !string.IsNullOrWhiteSpace(response.Direction)
        || !string.IsNullOrWhiteSpace(response.TimeRangeStart)
        || !string.IsNullOrWhiteSpace(response.TimeRangeEnd)
        || (response.Statuses?.Count > 0);

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeDestination(string? value)
    {
        var cleaned = Clean(value);
        if (string.IsNullOrWhiteSpace(cleaned)) return null;

        cleaned = cleaned.ToUpperInvariant();
        return cleaned.Length == 4
            ? AdvancedSearchViewModel.ConvertToIata(cleaned)
            : cleaned;
    }

    private static string? NormalizeDate(string? value)
    {
        var cleaned = Clean(value);
        if (string.IsNullOrWhiteSpace(cleaned)) return null;

        return DateTime.TryParse(cleaned, out var parsed)
            ? parsed.ToString("yyyy-MM-dd")
            : cleaned;
    }

    private static string? NormalizeTerminal(string? value)
    {
        var cleaned = Clean(value);
        if (string.IsNullOrWhiteSpace(cleaned)) return null;

        var digits = new string(cleaned.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? cleaned : digits;
    }

    private static string? NormalizeDirection(string? value)
    {
        var cleaned = Clean(value);
        if (string.IsNullOrWhiteSpace(cleaned)) return string.Empty;

        return cleaned.Equals("departure", StringComparison.OrdinalIgnoreCase) ? "Departure"
            : cleaned.Equals("arrival", StringComparison.OrdinalIgnoreCase) ? "Arrival"
            : string.Empty;
    }

    private static string? NormalizeTime(string? value)
    {
        var cleaned = Clean(value);
        if (string.IsNullOrWhiteSpace(cleaned)) return null;

        return TimeSpan.TryParse(cleaned, out var parsed)
            ? $"{parsed.Hours:00}:{parsed.Minutes:00}"
            : cleaned;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class AIQueryRequest
{
    public string? Query { get; set; }
}

public class DeepSeekResponse
{
    public List<DeepSeekChoice>? Choices { get; set; }
}

public class DeepSeekChoice
{
    public DeepSeekMessage? Message { get; set; }
}

public class DeepSeekMessage
{
    public string? Content { get; set; }
}

public class AiSearchFiltersResponse
{
    public bool IsSearchRequest { get; set; } = true;
    public string? Message { get; set; }
    public string? Flight { get; set; }
    public string? Airline { get; set; }
    public string? Destination { get; set; }
    public string? DepartureDate { get; set; }
    public string? ArrivalDate { get; set; }
    public string? Terminal { get; set; }
    public string? Direction { get; set; }
    public List<string> Statuses { get; set; } = new();
    public string? TimeRangeStart { get; set; }
    public string? TimeRangeEnd { get; set; }
}