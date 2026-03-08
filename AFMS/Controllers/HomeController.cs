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
            var systemPrompt = $@"You are a flight search assistant for an airport flight management system based at London Heathrow (LHR).
Extract flight search parameters from the user's natural language query and return ONLY a valid JSON object — no markdown, no explanation, no extra text.

Available JSON fields:
  flight         – flight number string (e.g. ""BA123"")
  airline        – full official airline name (e.g. ""British Airways""). ALWAYS use the full name, never abbreviate.
  destination    – IATA airport code of the non-LHR endpoint (e.g. ""DOH"", ""JFK"", ""DXB""). Leave empty if no specific airport.
  departureDate  – date string YYYY-MM-DD
  arrivalDate    – date string YYYY-MM-DD
  terminal       – terminal string (e.g. ""5"")
  direction      – ""Departure"" | ""Arrival"" | """" (empty = both)
  timeRangeStart – HH:mm
  timeRangeEnd   – HH:mm
  statuses       – array from [""Expected"",""Boarding"",""Departed"",""Arrived"",""Delayed"",""Canceled""]

AIRLINE NAME MAPPING (always use the full name on the right):
  BA / British / British Air → British Airways
  AA / American → American Airlines
  UA / United → United Airlines
  DL / Delta → Delta Air Lines
  AF / Air France → Air France
  LH / Lufthansa → Lufthansa
  EK / Emirates → Emirates
  QR / Qatar → Qatar Airways
  EY / Etihad → Etihad Airways
  VS / Virgin / Virgin Atlantic → Virgin Atlantic
  AC / Air Canada → Air Canada
  SQ / Singapore / Singapore Air → Singapore Airlines
  CX / Cathay / Cathay Pacific → Cathay Pacific
  KL / KLM → KLM
  IB / Iberia → Iberia
  AZ / Alitalia / ITA → ITA Airways
  TK / Turkish / Turkish Air → Turkish Airlines
  LX / Swiss → Swiss International Air Lines
  OS / Austrian → Austrian Airlines
  SK / SAS / Scandinavian → Scandinavian Airlines
  AY / Finnair → Finnair
  RY / Ryanair → Ryanair
  FR / Ryanair → Ryanair
  U2 / EZY / Easyjet → easyJet
  W6 / Wizz / Wizzair → Wizz Air
  BE / Flybe → Flybe
  ZI / Aigle / Aigle Azur → Aigle Azur
  AV / Avianca → Avianca
  CM / Copa → Copa Airlines
  LA / LATAM → LATAM Airlines
  G3 / Gol → GOL Linhas Aéreas
  NH / ANA → All Nippon Airways
  JL / JAL / Japan Air → Japan Airlines
  OZ / Asiana → Asiana Airlines
  KE / Korean Air → Korean Air
  CA / Air China → Air China
  MU / China Eastern → China Eastern Airlines
  CZ / China Southern → China Southern Airlines
  AI / Air India → Air India
  MS / EgyptAir → EgyptAir
  ET / Ethiopian → Ethiopian Airlines
  KQ / Kenya → Kenya Airways
  SA / SAA / South African → South African Airways
  QF / Qantas → Qantas
  NZ / Air New Zealand → Air New Zealand
  MH / Malaysia / Malaysia Air → Malaysia Airlines
  GA / Garuda → Garuda Indonesia

OTHER RULES:
- Direction: ""from heathrow"", ""departing"", ""leaving"", ""going to [airport]"" → ""Departure"". ""to heathrow"", ""arriving"", ""coming to heathrow"", ""landing"" → ""Arrival"".
- City/country to IATA: Qatar/Doha→DOH, New York→JFK, Dubai→DXB, Paris→CDG, Frankfurt→FRA, Tokyo→HND, Amsterdam→AMS, Madrid→MAD, Singapore→SIN, Los Angeles→LAX, Sydney→SYD, Toronto→YYZ, Chicago→ORD, Miami→MIA, Barcelona→BCN, Rome→FCO, Lisbon→LIS, Istanbul→IST, Athens→ATH, Cairo→CAI, Bangkok→BKK, Hong Kong→HKG, Seoul→ICN, Shanghai→PVG, Beijing→PEK, Kuala Lumpur→KUL, Mumbai→BOM, Delhi→DEL, Nairobi→NBO, Cape Town→CPT, Johannesburg→JNB, Sao Paulo→GRU, Buenos Aires→EZE, Mexico City→MEX, Vancouver→YVR, Montreal→YUL, Boston→BOS, Washington→IAD, San Francisco→SFO, Seattle→SEA, Dallas→DFW, Houston→IAH, Atlanta→ATL, Denver→DEN, Orlando→MCO, Las Vegas→LAS, Manchester→MAN, Edinburgh→EDI, Glasgow→GLA, Birmingham→BHX, Bristol→BRS, Dublin→DUB.
- If the user says ""today"", use {today} as the date.
- Return ONLY the JSON object, nothing else.";

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

            var parsedParams = JsonDocument.Parse(content).RootElement;
            return Ok(parsedParams);
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