using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using AFMS.Data;
using AFMS.Models;
using AFMS.Services;

namespace AFMS.Controllers;

public class HomeController : Controller
{
    private const string SearchOnlyMessage = "I can only help with flight searches. Try asking for a flight by airline, destination, flight number, date, time, terminal or status.";
    private const string DeepSeekPromptTemplateCacheKey = "deepseek_prompt_template";
    private static readonly object AiThrottleLock = new();
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Expected", "Boarding", "Departed", "Arrived", "Delayed", "Canceled"
    };

    private readonly AeroDataBoxService _aeroDataBoxService;
    private readonly FlightSearchService _flightSearchService;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        AeroDataBoxService aeroDataBoxService,
        FlightSearchService flightSearchService,
        ApplicationDbContext context,
        IConfiguration configuration,
        IMemoryCache cache,
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment environment,
        ILogger<HomeController> logger)
    {
        _aeroDataBoxService  = aeroDataBoxService;
        _flightSearchService = flightSearchService;
        _context             = context;
        _configuration       = configuration;
        _cache               = cache;
        _httpClientFactory   = httpClientFactory;
        _environment         = environment;
        _logger              = logger;
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

        // Fetch all DB flights once — used for MANAGE links and dashboard merge
        var allDbFlights = await _context.Flights.ToListAsync();

        // Build flight number → DB id lookup for the MANAGE column
        ViewBag.DbFlightIds = allDbFlights
            .GroupBy(f => f.FlightNumber)
            .ToDictionary(g => g.Key, g => g.First().Id);

        // Override API data with values from manually-edited DB flights
        foreach (var dbFlight in allDbFlights.Where(f => f.IsManualEntry))
        {
            var existing = sortedFlights.FirstOrDefault(f =>
                string.Equals(f.Number?.Trim(), dbFlight.FlightNumber.Trim(), StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                var lhrLeg = existing.Direction == "Departure" ? existing.Departure : existing.Arrival;
                if (lhrLeg != null)
                {
                    if (!string.IsNullOrEmpty(dbFlight.Gate))     lhrLeg.Gate     = dbFlight.Gate;
                    if (!string.IsNullOrEmpty(dbFlight.Terminal)) lhrLeg.Terminal = dbFlight.Terminal;
                }
                existing.Status = dbFlight.Status;
            }
        }

        // Add manually-entered flights that the live API doesn't know about
        var apiNumbers = sortedFlights
            .Select(f => f.Number?.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var dbFlight in allDbFlights.Where(f => f.IsManualEntry && !apiNumbers.Contains(f.FlightNumber.Trim())))
            sortedFlights.Add(CreateSyntheticFlight(dbFlight));

        // Re-sort so manual additions land in the right chronological position
        sortedFlights = sortedFlights
            .OrderBy(f => {
                var leg = f.Direction == "Departure" ? f.Departure : f.Arrival;
                return AdvancedSearchViewModel.ParseLocalDate(leg?.ScheduledTime?.Local) ?? DateTime.MaxValue;
            })
            .ThenBy(f => f.Number)
            .ToList();

        // Surface API error if the list is empty
        if (!sortedFlights.Any())
            ViewBag.ApiError = _cache.Get<string>(AFMS.Services.AeroDataBoxService.ApiErrorCacheKey);

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

        ValidateSearchModel(model);
        if (model.HasValidationErrors)
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

        ValidateSearchModel(model);
        if (model.HasValidationErrors)
            return PartialView("_AdvancedSearchResults", model);

        await _flightSearchService.ExecuteSearchAsync(model);
        return PartialView("_AdvancedSearchResults", model);
    }

    [HttpPost]
    [Route("Home/ProcessAIQuery")]
    public async Task<IActionResult> ProcessAIQuery([FromBody] AIQueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Query))
            return BadRequest(new { error = "Query is required" });

        var clientKey = GetAiClientKey();
        if (!TryBeginAiRequest(clientKey, out var retryAfterSeconds))
        {
            Response.Headers["Retry-After"] = retryAfterSeconds.ToString();
            _logger.LogWarning("AI query throttled for client {ClientKey}. Retry after {RetryAfterSeconds}s.", clientKey, retryAfterSeconds);
            return StatusCode(StatusCodes.Status429TooManyRequests, new { error = "Too many AI requests. Please wait a few seconds and try again." });
        }

        try
        {
            var apiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY")
                ?? _configuration["DeepSeek:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("DeepSeek API key is not configured.");
                return StatusCode(500, new { error = "DeepSeek API key not configured" });
            }

            var model = _configuration["DeepSeek:Model"] ?? "deepseek-chat";
            var systemPrompt = BuildDeepSeekSystemPrompt();
            var deepSeekClient = _httpClientFactory.CreateClient("DeepSeek");
            deepSeekClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = request.Query }
                },
                temperature = 0,
                max_tokens = 300
            };

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(GetDeepSeekTimeoutSeconds()));

            var response = await deepSeekClient.PostAsJsonAsync("chat/completions", requestBody, timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "DeepSeek returned {StatusCode} for client {ClientKey}. Details: {Details}",
                    (int)response.StatusCode,
                    clientKey,
                    Truncate(errorContent, 300));

                return StatusCode((int)response.StatusCode, new
                {
                    error = $"DeepSeek API error: {response.StatusCode}",
                    details = Truncate(errorContent, 300)
                });
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            DeepSeekResponse? jsonResponse;
            try
            {
                jsonResponse = JsonSerializer.Deserialize<DeepSeekResponse>(jsonContent, opts);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "DeepSeek returned invalid outer JSON for client {ClientKey}.", clientKey);
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "The AI service returned an invalid response." });
            }

            var content = jsonResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "{}";
            
            content = System.Text.RegularExpressions.Regex.Replace(content, @"```(?:json)?|```", "").Trim();

            AiSearchFiltersResponse parsedParams;
            try
            {
                parsedParams = JsonSerializer.Deserialize<AiSearchFiltersResponse>(content, opts) ?? new AiSearchFiltersResponse();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "DeepSeek returned invalid search JSON for client {ClientKey}. Payload: {Payload}", clientKey, Truncate(content, 300));
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "The AI could not turn that request into search filters. Please try again." });
            }

            var normalizedParams = NormalizeAiSearchFilters(parsedParams);
            _logger.LogInformation(
                "AI query processed for client {ClientKey}. SearchRequest={IsSearchRequest}, FilterCount={FilterCount}",
                clientKey,
                normalizedParams.IsSearchRequest,
                CountSearchFilters(normalizedParams));

            return Ok(normalizedParams);
        }
        catch (OperationCanceledException ex) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "DeepSeek request timed out for client {ClientKey}.", clientKey);
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { error = "DeepSeek request timed out" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process AI query for client {ClientKey}.", clientKey);
            return StatusCode(500, new { error = "Failed to process AI request" });
        }
        finally
        {
            EndAiRequest(clientKey);
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

    private static void ValidateSearchModel(AdvancedSearchViewModel model)
    {
        model.ValidationErrors.Clear();
        TimeSpan? startTime = null;
        TimeSpan? endTime = null;

        var hasAnyFilter =
            !string.IsNullOrWhiteSpace(model.Flight)
            || !string.IsNullOrWhiteSpace(model.Airline)
            || !string.IsNullOrWhiteSpace(model.Destination)
            || model.DepartureDate.HasValue
            || model.ArrivalDate.HasValue
            || !string.IsNullOrWhiteSpace(model.Terminal)
            || !string.IsNullOrWhiteSpace(model.Direction)
            || !string.IsNullOrWhiteSpace(model.TimeRangeStart)
            || !string.IsNullOrWhiteSpace(model.TimeRangeEnd)
            || model.Statuses.Any();

        if (!hasAnyFilter)
        {
            model.ValidationErrors.Add("Choose at least one filter before searching.");
            return;
        }

        var hasValidStartTime = string.IsNullOrWhiteSpace(model.TimeRangeStart);
        if (!hasValidStartTime)
            hasValidStartTime = TimeSpan.TryParse(model.TimeRangeStart, out var parsedStartTime) && (startTime = parsedStartTime).HasValue;

        var hasValidEndTime = string.IsNullOrWhiteSpace(model.TimeRangeEnd);
        if (!hasValidEndTime)
            hasValidEndTime = TimeSpan.TryParse(model.TimeRangeEnd, out var parsedEndTime) && (endTime = parsedEndTime).HasValue;

        if (!hasValidStartTime)
            model.ValidationErrors.Add("Enter a valid start time.");

        if (!hasValidEndTime)
            model.ValidationErrors.Add("Enter a valid end time.");

        if (hasValidStartTime && hasValidEndTime
            && !string.IsNullOrWhiteSpace(model.TimeRangeStart)
            && !string.IsNullOrWhiteSpace(model.TimeRangeEnd)
            && startTime.HasValue
            && endTime.HasValue
            && startTime.Value > endTime.Value)
        {
            model.ValidationErrors.Add("The start time cannot be later than the end time.");
        }

        if (model.DepartureDate.HasValue && model.ArrivalDate.HasValue
            && model.DepartureDate.Value.Date > model.ArrivalDate.Value.Date)
        {
            model.ValidationErrors.Add("Departure date cannot be after arrival date.");
        }
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
            response.Message ??= SearchOnlyMessage;
            return response;
        }

        response.IsSearchRequest = HasAnySearchFilters(response);

        if (!response.IsSearchRequest)
        {
            response.Message = SearchOnlyMessage;
        }

        return response;
    }

    private bool TryBeginAiRequest(string clientKey, out int retryAfterSeconds)
    {
        var inFlightKey = $"ai-chat:inflight:{clientKey}";
        var rateLimitKey = $"ai-chat:rate:{clientKey}";
        var maxRequestsPerMinute = Math.Max(1, _configuration.GetValue<int?>("DeepSeek:MaxRequestsPerMinute") ?? 5);
        var now = DateTime.UtcNow;

        lock (AiThrottleLock)
        {
            if (_cache.TryGetValue(inFlightKey, out _))
            {
                retryAfterSeconds = 2;
                return false;
            }

            var rateState = _cache.Get<AiRateLimitState>(rateLimitKey);
            if (rateState is null || now >= rateState.WindowEndsUtc)
            {
                rateState = new AiRateLimitState
                {
                    WindowEndsUtc = now.AddMinutes(1),
                    RequestCount = 0
                };
            }

            if (rateState.RequestCount >= maxRequestsPerMinute)
            {
                retryAfterSeconds = Math.Max(1, (int)Math.Ceiling((rateState.WindowEndsUtc - now).TotalSeconds));
                _cache.Set(rateLimitKey, rateState, rateState.WindowEndsUtc);
                return false;
            }

            rateState.RequestCount++;
            _cache.Set(rateLimitKey, rateState, rateState.WindowEndsUtc);
            _cache.Set(inFlightKey, true, TimeSpan.FromSeconds(GetDeepSeekTimeoutSeconds() + 1));

            retryAfterSeconds = 0;
            return true;
        }
    }

    private void EndAiRequest(string clientKey)
    {
        lock (AiThrottleLock)
        {
            _cache.Remove($"ai-chat:inflight:{clientKey}");
        }
    }

    private string GetAiClientKey()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        return string.IsNullOrWhiteSpace(ipAddress) ? "unknown-client" : ipAddress;
    }

    private string BuildDeepSeekSystemPrompt()
    {
        var promptTemplate = GetDeepSeekPromptTemplate();
        return promptTemplate
            .Replace("{today}", DateTime.UtcNow.ToString("yyyy-MM-dd"), StringComparison.Ordinal)
            .Replace("{allowedStatuses}", string.Join(",", AllowedStatuses.Select(status => $"\"{status}\"")), StringComparison.Ordinal)
            .Replace("{searchOnlyMessage}", SearchOnlyMessage, StringComparison.Ordinal);
    }

    private string GetDeepSeekPromptTemplate()
    {
        return _cache.GetOrCreate(DeepSeekPromptTemplateCacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

            var configuredPath = _configuration["DeepSeek:PromptFile"] ?? "Prompts/DeepSeekFlightSearchPrompt.txt";
            var promptPath = Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(_environment.ContentRootPath, configuredPath);

            if (System.IO.File.Exists(promptPath))
                return System.IO.File.ReadAllText(promptPath);

            _logger.LogWarning("DeepSeek prompt file was not found at {PromptPath}. Falling back to a built-in prompt.", promptPath);
            return "You fill Advanced Search filters for a London Heathrow flight search page. You are NOT a general chatbot. If the message is not clearly about searching or filtering flights, return JSON with isSearchRequest false and message {searchOnlyMessage}. If the message is about searching flights, return only valid JSON using the known search fields and statuses [{allowedStatuses}]. If the user says today, use {today}.";
        }) ?? string.Empty;
    }

    private int GetDeepSeekTimeoutSeconds() =>
        Math.Max(5, _configuration.GetValue<int?>("DeepSeek:TimeoutSeconds") ?? 15);

    private static int CountSearchFilters(AiSearchFiltersResponse response)
    {
        var count = 0;

        if (!string.IsNullOrWhiteSpace(response.Flight)) count++;
        if (!string.IsNullOrWhiteSpace(response.Airline)) count++;
        if (!string.IsNullOrWhiteSpace(response.Destination)) count++;
        if (!string.IsNullOrWhiteSpace(response.DepartureDate)) count++;
        if (!string.IsNullOrWhiteSpace(response.ArrivalDate)) count++;
        if (!string.IsNullOrWhiteSpace(response.Terminal)) count++;
        if (!string.IsNullOrWhiteSpace(response.Direction)) count++;
        if (!string.IsNullOrWhiteSpace(response.TimeRangeStart) || !string.IsNullOrWhiteSpace(response.TimeRangeEnd)) count++;
        if (response.Statuses?.Count > 0) count++;

        return count;
    }

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
            return value ?? string.Empty;

        return value[..maxLength] + "...";
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

    private static AeroDataBoxFlight CreateSyntheticFlight(Flight f) => new()
    {
        Number    = f.FlightNumber,
        Status    = f.Status,
        Direction = "Departure",
        Airline   = new Airline { Name = f.Airline },
        Departure = new FlightMovement
        {
            Airport = new Airport { Iata = "LHR", Icao = "EGLL", Name = "London Heathrow" },
            Gate     = f.Gate,
            Terminal = f.Terminal,
            Status   = f.Status,
            ScheduledTime = new ScheduledTime
            {
                Local = f.DepartureTime.ToString("yyyy-MM-ddTHH:mmzzz"),
                Utc   = f.DepartureTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mmZ")
            }
        },
        Arrival = new FlightMovement
        {
            Airport = new Airport { Iata = f.Destination, Name = f.Destination },
            ScheduledTime = new ScheduledTime
            {
                Local = f.ArrivalTime.ToString("yyyy-MM-ddTHH:mmzzz"),
                Utc   = f.ArrivalTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mmZ")
            }
        }
    };
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

public class AiRateLimitState
{
    public DateTime WindowEndsUtc { get; set; }
    public int RequestCount { get; set; }
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