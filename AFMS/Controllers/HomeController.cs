using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using AFMS.Data;
using AFMS.Models;
using AFMS.Services;

namespace AFMS.Controllers;

public class HomeController : Controller
{
    private const string LondonTimeZoneId = "Europe/London";
    private const string SearchOnlyMessage = "I can only help with flight searches. Try asking for a flight by airline, destination, flight number, date, time, terminal or status.";
    private const string DeepSeekPromptTemplateCacheKey = "deepseek_prompt_template";
    private const string AddFlightOnlyMessage = "I can only help with adding flights here. Share flight number, airline, destination, and departure time.";
    private const string DeepSeekAddFlightPromptTemplateCacheKey = "deepseek_add_flight_prompt_template";
    private static readonly object AiThrottleLock = new();
    private static readonly string[] RequiredAddFlightFields =
    [
        "flightNumber", "airline", "destination", "departureTime"
    ];
    private static readonly HashSet<string> SearchClearableFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "flight", "airline", "destination", "departureDate", "arrivalDate",
        "terminal", "direction", "timeRangeStart", "timeRangeEnd", "statuses"
    };
    private static readonly Dictionary<string, string> SearchClearFieldAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["status"] = "statuses",
        ["timeStart"] = "timeRangeStart",
        ["timeEnd"] = "timeRangeEnd",
        ["departure"] = "departureDate",
        ["arrival"] = "arrivalDate"
    };
    private static readonly Dictionary<string, string> AirportAliasToIata = new(StringComparer.OrdinalIgnoreCase)
    {
        ["heathrow"] = "LHR",
        ["london heathrow"] = "LHR",
        ["lhr"] = "LHR",
        ["egll"] = "LHR",
        ["gatwick"] = "LGW",
        ["london gatwick"] = "LGW",
        ["lgw"] = "LGW",
        ["stansted"] = "STN",
        ["london stansted"] = "STN",
        ["stn"] = "STN"
    };

    private readonly AeroDataBoxService _aeroDataBoxService;
    private readonly FlightSearchService _flightSearchService;
    private readonly ManualFlightMergeService _manualFlightMergeService;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        AeroDataBoxService aeroDataBoxService,
        FlightSearchService flightSearchService,
        ManualFlightMergeService manualFlightMergeService,
        ApplicationDbContext context,
        IConfiguration configuration,
        IMemoryCache cache,
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment environment,
        ILogger<HomeController> logger)
    {
        _aeroDataBoxService  = aeroDataBoxService;
        _flightSearchService = flightSearchService;
        _manualFlightMergeService = manualFlightMergeService;
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
        var londonTime = GetLondonNow();

        // Use cache to avoid hitting API rate limits (BASIC plan has strict limits)
        var cacheKey = $"flights_{airportCode}_{londonTime:yyyyMMddHH}";

        var flights = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            // Cache for 2 minutes to reduce API calls
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
            return await _aeroDataBoxService.GetAirportFlightsAsync(airportCode, londonTime);
        });

        var sortedFlights = SortFlightsByLhrLegTime(flights ?? []);

        // Fetch all DB flights once — used for MANAGE links and dashboard merge
        var allDbFlights = await _context.Flights.ToListAsync();

        // Build flight number → DB id lookup for the MANAGE column
        ViewBag.DbFlightIds = allDbFlights
            .Where(f => !string.IsNullOrWhiteSpace(f.FlightNumber))
            .GroupBy(f => NormalizeFlightNumber(f.FlightNumber)!)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

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
=======
        sortedFlights = _manualFlightMergeService
            .MergeManualFlights(sortedFlights, allDbFlights)
            .ToList();


        // Re-sort so manual additions land in the right chronological position
        sortedFlights = SortFlightsByLhrLegTime(sortedFlights);

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
            FlightStatusCatalog.NormalizeStatuses(statuses), timeRangeStart, timeRangeEnd,
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
            var normalizedContext = NormalizeAiSearchContext(request.SearchContext);
            var deepSeekClient = _httpClientFactory.CreateClient("DeepSeek");
            deepSeekClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            if (HasAnySearchContextFields(normalizedContext))
            {
                var contextJson = JsonSerializer.Serialize(
                    normalizedContext,
                    new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                messages.Add(new
                {
                    role = "user",
                    content = $"Search context: {contextJson}"
                });
            }

            messages.Add(new { role = "user", content = request.Query });

            var requestBody = new
            {
                model,
                messages,
                temperature = 0,
                max_tokens = 250
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

            var mergedParams = MergeWithAiSearchContext(parsedParams, normalizedContext, request.Query);
            var normalizedParams = NormalizeAiSearchFilters(mergedParams);
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

    [HttpPost]
    [Route("Home/ProcessAddFlightQuery")]
    public async Task<IActionResult> ProcessAddFlightQuery([FromBody] AIQueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Query))
            return BadRequest(new { error = "Query is required" });

        var clientKey = GetAiClientKey();
        if (!TryBeginAiRequest(clientKey, out var retryAfterSeconds))
        {
            Response.Headers["Retry-After"] = retryAfterSeconds.ToString();
            _logger.LogWarning("AI add-flight query throttled for client {ClientKey}. Retry after {RetryAfterSeconds}s.", clientKey, retryAfterSeconds);
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
            var systemPrompt = BuildDeepSeekAddFlightSystemPrompt();
            var normalizedContext = NormalizeAiAddFlightContext(request.AddFlightContext);
            var deepSeekClient = _httpClientFactory.CreateClient("DeepSeek");
            deepSeekClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            if (HasAnyAddFlightContextFields(normalizedContext))
            {
                var contextJson = JsonSerializer.Serialize(
                    normalizedContext,
                    new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                messages.Add(new
                {
                    role = "user",
                    content = $"Add-flight context: {contextJson}"
                });
            }

            messages.Add(new { role = "user", content = request.Query });

            var requestBody = new
            {
                model,
                messages,
                temperature = 0,
                max_tokens = 250
            };

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(GetDeepSeekTimeoutSeconds()));

            var response = await deepSeekClient.PostAsJsonAsync("chat/completions", requestBody, timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "DeepSeek add-flight request returned {StatusCode} for client {ClientKey}. Details: {Details}",
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
                _logger.LogWarning(ex, "DeepSeek returned invalid outer JSON for add-flight request (client {ClientKey}).", clientKey);
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "The AI service returned an invalid response." });
            }

            var content = jsonResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "{}";
            content = System.Text.RegularExpressions.Regex.Replace(content, @"```(?:json)?|```", "").Trim();

            AiAddFlightResponse parsedParams;
            try
            {
                parsedParams = JsonSerializer.Deserialize<AiAddFlightResponse>(content, opts) ?? new AiAddFlightResponse();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "DeepSeek returned invalid add-flight JSON for client {ClientKey}. Payload: {Payload}", clientKey, Truncate(content, 300));
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "The AI could not parse that into add-flight fields. Please try again." });
            }

            var mergedParams = MergeWithAiAddFlightContext(parsedParams, normalizedContext);
            var enrichedParams = EnrichAddFlightFromNaturalLanguage(request.Query, mergedParams);
            var normalizedParams = NormalizeAiAddFlightResponse(enrichedParams);
            _logger.LogInformation(
                "AI add-flight query processed for client {ClientKey}. AddFlightRequest={IsAddFlightRequest}, MissingRequired={MissingRequired}",
                clientKey,
                normalizedParams.IsAddFlightRequest,
                string.Join(",", normalizedParams.MissingRequiredFields));

            return Ok(normalizedParams);
        }
        catch (OperationCanceledException ex) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "DeepSeek add-flight request timed out for client {ClientKey}.", clientKey);
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { error = "DeepSeek request timed out" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process add-flight AI query for client {ClientKey}.", clientKey);
            return StatusCode(500, new { error = "Failed to process add-flight AI request" });
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
        response.Statuses = NormalizeSearchStatuses(response.Statuses);
        response.ClearFields = NormalizeSearchClearFields(response.ClearFields);
        response.Message = Clean(response.Message);

        if (!response.IsSearchRequest && !HasAnySearchFilters(response) && response.ClearFields.Count == 0)
        {
            response.Message ??= SearchOnlyMessage;
            return response;
        }

        response.IsSearchRequest = HasAnySearchFilters(response) || response.ClearFields.Count > 0;

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
            .Replace("{allowedStatuses}", string.Join(",", FlightStatusCatalog.Values.Select(status => $"\"{status}\"")), StringComparison.Ordinal)
            .Replace("{searchOnlyMessage}", SearchOnlyMessage, StringComparison.Ordinal);
    }

    private string BuildDeepSeekAddFlightSystemPrompt()
    {
        var utcToday = DateTime.UtcNow.Date;
        var promptTemplate = GetDeepSeekAddFlightPromptTemplate();

        return promptTemplate
            .Replace("{today}", utcToday.ToString("yyyy-MM-dd"), StringComparison.Ordinal)
            .Replace("{tomorrow}", utcToday.AddDays(1).ToString("yyyy-MM-dd"), StringComparison.Ordinal)
            .Replace("{addFlightOnlyMessage}", AddFlightOnlyMessage, StringComparison.Ordinal);
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
            return "You fill Advanced Search filters for Heathrow. Not a general chatbot. If the message is not about search/filtering flights, return JSON only: {\"isSearchRequest\":false,\"message\":\"{searchOnlyMessage}\"}. Otherwise return JSON only with: isSearchRequest, message, flight, airline, destination, departureDate, arrivalDate, terminal, direction, timeRangeStart, timeRangeEnd, statuses, clearFields. Keep existing values unless changed or cleared. Use clearFields for resets. today => {today}.";
        }) ?? string.Empty;
    }

    private string GetDeepSeekAddFlightPromptTemplate()
    {
        return _cache.GetOrCreate(DeepSeekAddFlightPromptTemplateCacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

            var configuredPath = _configuration["DeepSeek:AddFlightPromptFile"] ?? "Prompts/DeepSeekAddFlightPrompt.txt";
            var promptPath = Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(_environment.ContentRootPath, configuredPath);

            if (System.IO.File.Exists(promptPath))
                return System.IO.File.ReadAllText(promptPath);

            _logger.LogWarning("Add-flight DeepSeek prompt file was not found at {PromptPath}. Falling back to a built-in prompt.", promptPath);
            return "You fill the Add Flight form for Heathrow. Not a general chatbot. Context JSON contains only fields already filled; any field not present is blank. If the message is not about adding/editing a flight entry, return JSON only: {\"isAddFlightRequest\":false,\"message\":\"{addFlightOnlyMessage}\"}. Otherwise return JSON only with: isAddFlightRequest, message, flightNumber, airline, destination, departureTime, arrivalTime, gate, terminal, arrivalEstimated, gateEstimated, terminalEstimated, missingRequiredFields. Keep existing values unless changed. Generate plausible missing values when asked. today => {today}, tomorrow => {tomorrow}.";
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

    private static bool HasAnyAddFlightFields(AiAddFlightResponse response) =>
        !string.IsNullOrWhiteSpace(response.FlightNumber)
        || !string.IsNullOrWhiteSpace(response.Airline)
        || !string.IsNullOrWhiteSpace(response.Destination)
        || !string.IsNullOrWhiteSpace(response.DepartureTime)
        || !string.IsNullOrWhiteSpace(response.ArrivalTime)
        || !string.IsNullOrWhiteSpace(response.Gate)
        || !string.IsNullOrWhiteSpace(response.Terminal);

    private static bool HasAnyAddFlightContextFields(AiAddFlightContext? context) =>
        context is not null
        && (!string.IsNullOrWhiteSpace(context.FlightNumber)
            || !string.IsNullOrWhiteSpace(context.Airline)
            || !string.IsNullOrWhiteSpace(context.Destination)
            || !string.IsNullOrWhiteSpace(context.DepartureTime)
            || !string.IsNullOrWhiteSpace(context.ArrivalTime)
            || !string.IsNullOrWhiteSpace(context.Gate)
            || !string.IsNullOrWhiteSpace(context.Terminal));

    private static bool HasAnySearchContextFields(AiSearchContext? context) =>
        context is not null
        && (!string.IsNullOrWhiteSpace(context.Flight)
            || !string.IsNullOrWhiteSpace(context.Airline)
            || !string.IsNullOrWhiteSpace(context.Destination)
            || !string.IsNullOrWhiteSpace(context.DepartureDate)
            || !string.IsNullOrWhiteSpace(context.ArrivalDate)
            || !string.IsNullOrWhiteSpace(context.Terminal)
            || !string.IsNullOrWhiteSpace(context.Direction)
            || !string.IsNullOrWhiteSpace(context.TimeRangeStart)
            || !string.IsNullOrWhiteSpace(context.TimeRangeEnd)
            || (context.Statuses?.Count ?? 0) > 0);

    private static AiSearchContext? NormalizeAiSearchContext(AiSearchContext? context)
    {
        if (context is null)
            return null;

        var normalized = new AiSearchContext
        {
            Flight = NormalizeFlightNumber(context.Flight),
            Airline = Clean(context.Airline),
            Destination = NormalizeDestination(context.Destination),
            DepartureDate = NormalizeDate(context.DepartureDate),
            ArrivalDate = NormalizeDate(context.ArrivalDate),
            Terminal = NormalizeTerminal(context.Terminal),
            Direction = NormalizeDirection(context.Direction),
            TimeRangeStart = NormalizeTime(context.TimeRangeStart),
            TimeRangeEnd = NormalizeTime(context.TimeRangeEnd),
            Statuses = NormalizeSearchStatuses(context.Statuses)
        };

        return HasAnySearchContextFields(normalized) ? normalized : null;
    }

    private static AiSearchFiltersResponse MergeWithAiSearchContext(AiSearchFiltersResponse aiResponse, AiSearchContext? context, string? query)
    {
        var clearFields = NormalizeSearchClearFields((aiResponse.ClearFields ?? new List<string>())
            .Concat(ExtractClearFieldsFromQuery(query))
            .ToList());
        aiResponse.ClearFields = clearFields;

        if (!HasAnySearchContextFields(context))
            return aiResponse;

        bool shouldClear(string fieldName) => clearFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase);

        aiResponse.Flight = shouldClear("flight")
            ? null
            : string.IsNullOrWhiteSpace(aiResponse.Flight) ? context!.Flight : aiResponse.Flight;

        aiResponse.Airline = shouldClear("airline")
            ? null
            : string.IsNullOrWhiteSpace(aiResponse.Airline) ? context!.Airline : aiResponse.Airline;

        aiResponse.Destination = shouldClear("destination")
            ? null
            : string.IsNullOrWhiteSpace(aiResponse.Destination) ? context!.Destination : aiResponse.Destination;

        aiResponse.DepartureDate = shouldClear("departureDate")
            ? null
            : string.IsNullOrWhiteSpace(aiResponse.DepartureDate) ? context!.DepartureDate : aiResponse.DepartureDate;

        aiResponse.ArrivalDate = shouldClear("arrivalDate")
            ? null
            : string.IsNullOrWhiteSpace(aiResponse.ArrivalDate) ? context!.ArrivalDate : aiResponse.ArrivalDate;

        aiResponse.Terminal = shouldClear("terminal")
            ? null
            : string.IsNullOrWhiteSpace(aiResponse.Terminal) ? context!.Terminal : aiResponse.Terminal;

        aiResponse.Direction = shouldClear("direction")
            ? string.Empty
            : string.IsNullOrWhiteSpace(aiResponse.Direction) ? context!.Direction : aiResponse.Direction;

        aiResponse.TimeRangeStart = shouldClear("timeRangeStart")
            ? null
            : string.IsNullOrWhiteSpace(aiResponse.TimeRangeStart) ? context!.TimeRangeStart : aiResponse.TimeRangeStart;

        aiResponse.TimeRangeEnd = shouldClear("timeRangeEnd")
            ? null
            : string.IsNullOrWhiteSpace(aiResponse.TimeRangeEnd) ? context!.TimeRangeEnd : aiResponse.TimeRangeEnd;

        if (shouldClear("statuses"))
        {
            aiResponse.Statuses = new List<string>();
        }
        else if ((aiResponse.Statuses?.Count ?? 0) == 0 && (context!.Statuses?.Count ?? 0) > 0)
        {
            aiResponse.Statuses = (context.Statuses ?? new List<string>()).ToList();
        }

        return aiResponse;
    }

    private static List<string> NormalizeSearchStatuses(IEnumerable<string>? statuses) =>
        FlightStatusCatalog.NormalizeStatuses(statuses);

    private static List<string> NormalizeSearchClearFields(IEnumerable<string>? clearFields)
    {
        var result = new List<string>();

        foreach (var raw in clearFields ?? Array.Empty<string>())
        {
            var cleaned = Clean(raw);
            if (string.IsNullOrWhiteSpace(cleaned))
                continue;

            if (SearchClearFieldAliases.TryGetValue(cleaned, out var mapped))
                cleaned = mapped;

            if (SearchClearableFields.Contains(cleaned) && !result.Contains(cleaned, StringComparer.OrdinalIgnoreCase))
                result.Add(cleaned);
        }

        return result;
    }

    private static List<string> ExtractClearFieldsFromQuery(string? query)
    {
        var text = Clean(query)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        var clearFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var hasClearIntent = Regex.IsMatch(text, @"\b(clear|remove|reset|drop|delete|without|show all)\b", RegexOptions.IgnoreCase);

        if (!hasClearIntent)
            return clearFields.ToList();

        if (Regex.IsMatch(text, @"\b(clear|reset|remove)\s+(all|everything|all filters|filters)\b", RegexOptions.IgnoreCase))
        {
            foreach (var field in SearchClearableFields)
                clearFields.Add(field);
            return clearFields.ToList();
        }

        if (Regex.IsMatch(text, @"\b(show\s+all\s+status(?:es)?|remove\s+status(?:es)?|clear\s+status(?:es)?)\b", RegexOptions.IgnoreCase))
            clearFields.Add("statuses");

        if (text.Contains("terminal", StringComparison.OrdinalIgnoreCase)) clearFields.Add("terminal");
        if (Regex.IsMatch(text, @"\b(departure\s+date|depart\s+date|departure filter)\b", RegexOptions.IgnoreCase)) clearFields.Add("departureDate");
        if (Regex.IsMatch(text, @"\b(arrival\s+date|arrival filter)\b", RegexOptions.IgnoreCase)) clearFields.Add("arrivalDate");
        if (Regex.IsMatch(text, @"\b(time\s+range|from\s+time|to\s+time|time filter)\b", RegexOptions.IgnoreCase))
        {
            clearFields.Add("timeRangeStart");
            clearFields.Add("timeRangeEnd");
        }
        if (text.Contains("direction", StringComparison.OrdinalIgnoreCase) || text.Contains("arrival/departure", StringComparison.OrdinalIgnoreCase)) clearFields.Add("direction");
        if (text.Contains("flight", StringComparison.OrdinalIgnoreCase) && text.Contains("number", StringComparison.OrdinalIgnoreCase)) clearFields.Add("flight");
        if (text.Contains("airline", StringComparison.OrdinalIgnoreCase)) clearFields.Add("airline");
        if (text.Contains("destination", StringComparison.OrdinalIgnoreCase)) clearFields.Add("destination");

        return clearFields.ToList();
    }

    private static AiAddFlightContext? NormalizeAiAddFlightContext(AiAddFlightContext? context)
    {
        if (context is null)
            return null;

        var normalized = new AiAddFlightContext
        {
            FlightNumber = NormalizeFlightNumber(context.FlightNumber),
            Airline = Clean(context.Airline),
            Destination = NormalizeDestination(context.Destination),
            DepartureTime = NormalizeDateTimeForForm(context.DepartureTime),
            ArrivalTime = NormalizeDateTimeForForm(context.ArrivalTime),
            Gate = NormalizeGate(context.Gate),
            Terminal = NormalizeTerminalForAddForm(context.Terminal),
            FilledFields = context.FilledFields?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>()
        };

        return normalized;
    }

    private static AiAddFlightResponse MergeWithAiAddFlightContext(AiAddFlightResponse aiResponse, AiAddFlightContext? context)
    {
        if (!HasAnyAddFlightContextFields(context))
            return aiResponse;

        aiResponse.FlightNumber = string.IsNullOrWhiteSpace(aiResponse.FlightNumber)
            ? context!.FlightNumber
            : aiResponse.FlightNumber;
        aiResponse.Airline = string.IsNullOrWhiteSpace(aiResponse.Airline)
            ? context!.Airline
            : aiResponse.Airline;
        aiResponse.Destination = string.IsNullOrWhiteSpace(aiResponse.Destination)
            ? context!.Destination
            : aiResponse.Destination;
        aiResponse.DepartureTime = string.IsNullOrWhiteSpace(aiResponse.DepartureTime)
            ? context!.DepartureTime
            : aiResponse.DepartureTime;

        if (string.IsNullOrWhiteSpace(aiResponse.ArrivalTime))
        {
            aiResponse.ArrivalTime = context!.ArrivalTime;
            if (!string.IsNullOrWhiteSpace(aiResponse.ArrivalTime))
                aiResponse.ArrivalEstimated = false;
        }

        if (string.IsNullOrWhiteSpace(aiResponse.Gate))
        {
            aiResponse.Gate = context!.Gate;
            if (!string.IsNullOrWhiteSpace(aiResponse.Gate))
                aiResponse.GateEstimated = false;
        }

        if (string.IsNullOrWhiteSpace(aiResponse.Terminal))
        {
            aiResponse.Terminal = context!.Terminal;
            if (!string.IsNullOrWhiteSpace(aiResponse.Terminal))
                aiResponse.TerminalEstimated = false;
        }

        return aiResponse;
    }

    private static AiAddFlightResponse EnrichAddFlightFromNaturalLanguage(string? query, AiAddFlightResponse response)
    {
        var cleanedQuery = Clean(query);
        if (string.IsNullOrWhiteSpace(cleanedQuery))
            return response;

        if (string.IsNullOrWhiteSpace(response.FlightNumber))
            response.FlightNumber = TryExtractFlightNumber(cleanedQuery);

        if (string.IsNullOrWhiteSpace(response.Airline))
            response.Airline = TryExtractAirline(cleanedQuery);

        if (string.IsNullOrWhiteSpace(response.Destination))
            response.Destination = TryExtractDestination(cleanedQuery);

        if (string.IsNullOrWhiteSpace(response.DepartureTime))
            response.DepartureTime = TryExtractDepartureDateTime(cleanedQuery);

        if (string.IsNullOrWhiteSpace(response.Terminal))
            response.Terminal = TryExtractTerminal(cleanedQuery);

        if (string.IsNullOrWhiteSpace(response.Gate))
            response.Gate = TryExtractGate(cleanedQuery);

        response = ApplyRequestedAddFlightFieldGeneration(cleanedQuery, response);

        return response;
    }

    private static AiAddFlightResponse ApplyRequestedAddFlightFieldGeneration(string query, AiAddFlightResponse response)
    {
        if (!HasGenerationIntent(query))
            return response;

        var lower = query.ToLowerInvariant();
        var wantsAllFields = Regex.IsMatch(lower, @"\b(all|these|required)\s+(fields?|details?)\b", RegexOptions.IgnoreCase)
            || Regex.IsMatch(lower, @"\b(fill|populate|complete|set|add|generate|auto\s*fill|autofill|randomize|random)\s+(the\s+)?(blank|empty|missing)?\s*(fields?|details?)\b", RegexOptions.IgnoreCase)
            || lower.Contains("enter these fields", StringComparison.OrdinalIgnoreCase)
            || lower.Contains("fill the fields", StringComparison.OrdinalIgnoreCase)
            || lower.Contains("fill the blank fields", StringComparison.OrdinalIgnoreCase)
            || lower.Contains("auto fill", StringComparison.OrdinalIgnoreCase)
            || lower.Contains("autofill", StringComparison.OrdinalIgnoreCase)
            || lower.Contains("random gate", StringComparison.OrdinalIgnoreCase)
            || lower.Contains("random terminal", StringComparison.OrdinalIgnoreCase)
            || lower.Contains("random departure", StringComparison.OrdinalIgnoreCase)
            || lower.Contains("random arrival", StringComparison.OrdinalIgnoreCase);

        bool wantsField(params string[] terms) => wantsAllFields || terms.Any(term => lower.Contains(term, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(response.FlightNumber) && wantsField("flight name", "flight number", "flight"))
            response.FlightNumber = GenerateFlightNumber(response.Airline);

        if (string.IsNullOrWhiteSpace(response.Airline) && wantsField("airline"))
            response.Airline = "British Airways";

        if (string.IsNullOrWhiteSpace(response.Destination) && wantsField("destination", "route"))
            response.Destination = "CDG";

        if (string.IsNullOrWhiteSpace(response.DepartureTime) && wantsField("departure", "departure time", "time"))
        {
            var londonNow = GetLondonNow();
            response.DepartureTime = londonNow.AddHours(2).ToString("yyyy-MM-ddTHH:mm");
        }

        if (string.IsNullOrWhiteSpace(response.Message))
            response.Message = "I generated the requested missing flight fields. You can edit any value before saving.";

        return response;
    }

    private static bool HasGenerationIntent(string query) =>
        Regex.IsMatch(query, @"\b(generate|auto\s*fill|autofill|make up|create|enter|fill|populate|complete|set|randomize|random|choose|pick|change|update|replace)\b", RegexOptions.IgnoreCase)
        || query.Contains("fill the fields", StringComparison.OrdinalIgnoreCase)
        || query.Contains("fill the blank fields", StringComparison.OrdinalIgnoreCase)
        || query.Contains("change the", StringComparison.OrdinalIgnoreCase)
        || query.Contains("update the", StringComparison.OrdinalIgnoreCase)
        || query.Contains("replace the", StringComparison.OrdinalIgnoreCase);

    private static string GenerateFlightNumber(string? airline)
    {
        var letters = new string((airline ?? string.Empty)
            .Where(char.IsLetter)
            .Select(char.ToUpperInvariant)
            .Take(2)
            .ToArray());

        var prefix = letters.Length >= 2 ? letters : "BA";
        var suffix = (DateTime.UtcNow.Ticks % 9000) + 1000;
        return $"{prefix}{suffix}";
    }

    private static string? TryExtractFlightNumber(string text)
    {
        var matches = Regex.Matches(text, @"\b([A-Za-z]{2,5}\s?\d{1,4}[A-Za-z]?)\b", RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            var raw = match.Groups[1].Value;
            var normalized = NormalizeFlightNumber(raw);
            if (string.IsNullOrWhiteSpace(normalized))
                continue;

            if (!normalized.Any(char.IsLetter) || !normalized.Any(char.IsDigit))
                continue;

            return normalized;
        }

        return null;
    }

    private static string? TryExtractAirline(string text)
    {
        var match = Regex.Match(
            text,
            @"\b([A-Za-z][A-Za-z\s&\-]{1,40}(?:airlines?|airways|air))\b",
            RegexOptions.IgnoreCase);

        if (!match.Success)
            return null;

        var airline = match.Groups[1].Value.Trim();
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(airline.ToLowerInvariant());
    }

    private static string? TryExtractDestination(string text)
    {
        var segments = text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            var lowered = segment.ToLowerInvariant();
            var toIndex = lowered.LastIndexOf(" to ", StringComparison.Ordinal);
            if (toIndex < 0)
                continue;

            var originCandidate = segment[..toIndex].Trim();
            var destinationCandidate = segment[(toIndex + 4)..].Trim();

            var normalizedTo = NormalizeDestinationCandidate(destinationCandidate);
            if (IsHeathrowAlias(normalizedTo))
            {
                // Add Flight only creates outgoing flights from Heathrow, so reverse route phrasing
                // like "Portugal to Heathrow" should map destination to the non-Heathrow side.
                var normalizedFrom = NormalizeDestinationCandidate(originCandidate);
                if (!string.IsNullOrWhiteSpace(normalizedFrom) && !IsHeathrowAlias(normalizedFrom))
                    return normalizedFrom;
            }

            if (!string.IsNullOrWhiteSpace(normalizedTo))
                return normalizedTo;
        }

        var toMatch = Regex.Match(
            text,
            @"\bto\s+([A-Za-z][A-Za-z\s]{1,40}?)(?=\s+at\s+|\s+on\s+|\s+gate\b|\s+terminal\b|,|$)",
            RegexOptions.IgnoreCase);

        if (toMatch.Success)
        {
            var normalized = NormalizeDestinationCandidate(toMatch.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(normalized))
                return normalized;
        }

        return null;
    }

    private static string? NormalizeDestinationCandidate(string? candidate)
    {
        var cleaned = Clean(candidate);
        if (string.IsNullOrWhiteSpace(cleaned))
            return null;

        var normalizedAliasKey = cleaned.ToLowerInvariant();
        if (AirportAliasToIata.TryGetValue(normalizedAliasKey, out var aliasIata))
            return aliasIata;

        var compact = new string(cleaned.Where(char.IsLetter).ToArray());
        if (AirportAliasToIata.TryGetValue(compact, out var compactAliasIata))
            return compactAliasIata;

        if (Regex.IsMatch(cleaned, @"^[A-Za-z]{3}$"))
            return cleaned.ToUpperInvariant();

        if (Regex.IsMatch(cleaned, @"^[A-Za-z]{4}$"))
            return AdvancedSearchViewModel.ConvertToIata(cleaned.ToUpperInvariant());

        return cleaned;
    }

    private static bool IsHeathrowAlias(string? destination)
    {
        if (string.IsNullOrWhiteSpace(destination))
            return false;

        var normalized = destination.Trim().ToUpperInvariant();
        return normalized is "LHR" or "EGLL" or "HEATHROW" or "LONDON HEATHROW";
    }

    private static string? TryExtractDepartureDateTime(string text)
    {
        var isoMatch = Regex.Match(text, @"\b(\d{4}-\d{2}-\d{2}T\d{2}:\d{2})\b", RegexOptions.IgnoreCase);
        if (isoMatch.Success && TryParseDateTimeLocal(isoMatch.Groups[1].Value, out var isoParsed))
            return isoParsed.ToString("yyyy-MM-ddTHH:mm");

        var londonNow = GetLondonNow();
        var baseDate = londonNow.Date;
        var lower = text.ToLowerInvariant();

        if (lower.Contains("tomorrow", StringComparison.Ordinal))
            baseDate = baseDate.AddDays(1);
        else if (lower.Contains("yesterday", StringComparison.Ordinal))
            baseDate = baseDate.AddDays(-1);

        var amPmMatch = Regex.Match(text, @"\b(1[0-2]|0?[1-9])(?::([0-5]\d))?\s*(am|pm)\b", RegexOptions.IgnoreCase);
        if (amPmMatch.Success)
        {
            var hour = int.Parse(amPmMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            var minute = amPmMatch.Groups[2].Success
                ? int.Parse(amPmMatch.Groups[2].Value, CultureInfo.InvariantCulture)
                : 0;
            var meridian = amPmMatch.Groups[3].Value.ToLowerInvariant();

            if (meridian == "pm" && hour < 12) hour += 12;
            if (meridian == "am" && hour == 12) hour = 0;

            return baseDate.AddHours(hour).AddMinutes(minute).ToString("yyyy-MM-ddTHH:mm");
        }

        var hhmmMatch = Regex.Match(text, @"\b([01]?\d|2[0-3]):([0-5]\d)\b");
        if (hhmmMatch.Success)
        {
            var hour = int.Parse(hhmmMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            var minute = int.Parse(hhmmMatch.Groups[2].Value, CultureInfo.InvariantCulture);
            return baseDate.AddHours(hour).AddMinutes(minute).ToString("yyyy-MM-ddTHH:mm");
        }

        return null;
    }

    private static string? TryExtractTerminal(string text)
    {
        var match = Regex.Match(text, @"\bterminal\s*([1-5])\b", RegexOptions.IgnoreCase);
        if (match.Success)
            return match.Groups[1].Value;

        var shortMatch = Regex.Match(text, @"\bt\s*([1-5])\b", RegexOptions.IgnoreCase);
        return shortMatch.Success ? shortMatch.Groups[1].Value : null;
    }

    private static string? TryExtractGate(string text)
    {
        var labelled = Regex.Match(text, @"\bgate\s*([A-Za-z]\d{1,2})\b", RegexOptions.IgnoreCase);
        if (labelled.Success)
            return labelled.Groups[1].Value;

        var standalone = Regex.Match(text, @"\b([A-SU-Z]\d{1,2})\b", RegexOptions.IgnoreCase);
        return standalone.Success ? standalone.Groups[1].Value : null;
    }

    private static AiAddFlightResponse NormalizeAiAddFlightResponse(AiAddFlightResponse response)
    {
        response.FlightNumber = NormalizeFlightNumber(response.FlightNumber);
        response.Airline = Clean(response.Airline);
        response.Destination = NormalizeDestination(response.Destination);
        response.DepartureTime = NormalizeDateTimeForForm(response.DepartureTime);
        response.ArrivalTime = NormalizeDateTimeForForm(response.ArrivalTime);
        response.Gate = NormalizeGate(response.Gate);
        response.Terminal = NormalizeTerminalForAddForm(response.Terminal);
        response.Message = Clean(response.Message);

        if (!response.IsAddFlightRequest && !HasAnyAddFlightFields(response))
        {
            response.MissingRequiredFields = RequiredAddFlightFields.ToList();
            response.Message ??= AddFlightOnlyMessage;
            return response;
        }

        response.IsAddFlightRequest = true;

        if (TryParseDateTimeLocal(response.DepartureTime, out var departureTime))
        {
            if (!TryParseDateTimeLocal(response.ArrivalTime, out var arrivalTime) || arrivalTime <= departureTime)
            {
                response.ArrivalTime = departureTime.Add(EstimateArrivalOffset(response.Destination)).ToString("yyyy-MM-ddTHH:mm");
                response.ArrivalEstimated = true;
            }

            if (string.IsNullOrWhiteSpace(response.Terminal))
            {
                response.Terminal = EstimateTerminal(response.Airline);
                response.TerminalEstimated = true;
            }

            if (string.IsNullOrWhiteSpace(response.Gate))
            {
                response.Gate = EstimateGate(response.Terminal, response.FlightNumber);
                response.GateEstimated = true;
            }
        }
        else
        {
            response.ArrivalTime = null;
            response.ArrivalEstimated = false;
        }

        response.MissingRequiredFields = BuildMissingRequiredFields(response);

        if (response.MissingRequiredFields.Count > 0)
        {
            response.Message ??= "I still need the required details before this flight can be saved.";
        }
        else if (string.IsNullOrWhiteSpace(response.Message))
        {
            response.Message = "Flight details filled. Review and save when ready.";
        }

        return response;
    }

    private static TimeSpan EstimateArrivalOffset(string? destination)
    {
        var normalized = NormalizeDestinationCandidate(destination)?.ToUpperInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalized))
            return TimeSpan.FromHours(2);

        if (normalized is "LIS" or "OPO" or "FAO" || normalized.Contains("PORTUGAL", StringComparison.Ordinal))
            return TimeSpan.FromMinutes(160);

        return TimeSpan.FromHours(2);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTime GetLondonNow()
    {
        var londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById(LondonTimeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, londonTimeZone);
    }

    private static List<AeroDataBoxFlight> SortFlightsByLhrLegTime(IEnumerable<AeroDataBoxFlight> flights) =>
        flights
            .OrderBy(f =>
            {
                var leg = f.Direction == "Departure" ? f.Departure : f.Arrival;
                return AdvancedSearchViewModel.ParseLocalDate(leg?.ScheduledTime?.Local) ?? DateTime.MaxValue;
            })
            .ThenBy(f => f.Number)
            .ToList();

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

    private static string? NormalizeFlightNumber(string? value)
    {
        var cleaned = Clean(value);
        if (string.IsNullOrWhiteSpace(cleaned)) return null;

        return cleaned.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
    }

    private static string? NormalizeGate(string? value)
    {
        var cleaned = Clean(value);
        if (string.IsNullOrWhiteSpace(cleaned)) return null;

        var normalized = cleaned
            .ToUpperInvariant()
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        return normalized.Length > 6 ? normalized[..6] : normalized;
    }

    private static string? NormalizeTerminalForAddForm(string? value)
    {
        var normalized = NormalizeTerminal(value);
        if (string.IsNullOrWhiteSpace(normalized)) return null;

        return int.TryParse(normalized, out var number) && number is >= 1 and <= 5
            ? number.ToString(CultureInfo.InvariantCulture)
            : null;
    }

    private static string? NormalizeDateTimeForForm(string? value)
    {
        if (!TryParseDateTimeLocal(value, out var parsedDateTime))
            return null;

        return parsedDateTime.ToString("yyyy-MM-ddTHH:mm");
    }

    private static bool TryParseDateTimeLocal(string? value, out DateTime parsed)
    {
        parsed = default;
        var cleaned = Clean(value);
        if (string.IsNullOrWhiteSpace(cleaned))
            return false;

        string[] formats =
        [
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-dd"
        ];

        if (DateTime.TryParseExact(cleaned, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsed))
        {
            if (cleaned.Length == 10)
                parsed = parsed.Date.AddHours(12);

            return true;
        }

        return DateTime.TryParse(cleaned, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsed)
            || DateTime.TryParse(cleaned, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out parsed);
    }

    private static string EstimateTerminal(string? airline)
    {
        var normalizedAirline = (airline ?? string.Empty).ToLowerInvariant();

        if (normalizedAirline.Contains("british airways", StringComparison.Ordinal))
            return "5";

        if (normalizedAirline.Contains("virgin atlantic", StringComparison.Ordinal))
            return "3";

        if (normalizedAirline.Contains("emirates", StringComparison.Ordinal)
            || normalizedAirline.Contains("qatar", StringComparison.Ordinal)
            || normalizedAirline.Contains("etihad", StringComparison.Ordinal)
            || normalizedAirline.Contains("air india", StringComparison.Ordinal))
        {
            return "4";
        }

        return "3";
    }

    private static string EstimateGate(string? terminal, string? flightNumber)
    {
        var prefix = terminal switch
        {
            "1" => "A",
            "2" => "B",
            "3" => "C",
            "4" => "D",
            "5" => "A",
            _ => "C"
        };

        var gateNumber = 12;
        var digits = new string((flightNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length > 0)
        {
            var tailDigits = digits.Length > 2 ? digits[^2..] : digits;
            if (int.TryParse(tailDigits, out var parsedDigits))
                gateNumber = Math.Clamp(parsedDigits % 39 + 1, 1, 40);
        }

        return $"{prefix}{gateNumber:00}";
    }

    private static List<string> BuildMissingRequiredFields(AiAddFlightResponse response)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(response.FlightNumber)) missing.Add("flightNumber");
        if (string.IsNullOrWhiteSpace(response.Airline)) missing.Add("airline");
        if (string.IsNullOrWhiteSpace(response.Destination)) missing.Add("destination");
        if (string.IsNullOrWhiteSpace(response.DepartureTime)) missing.Add("departureTime");

        return missing;
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
    public AiSearchContext? SearchContext { get; set; }
    public AiAddFlightContext? AddFlightContext { get; set; }
}

public class AiSearchContext
{
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

public class AiAddFlightContext
{
    public string? FlightNumber { get; set; }
    public string? Airline { get; set; }
    public string? Destination { get; set; }
    public string? DepartureTime { get; set; }
    public string? ArrivalTime { get; set; }
    public string? Gate { get; set; }
    public string? Terminal { get; set; }
    public List<string> FilledFields { get; set; } = new();
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
    public List<string> ClearFields { get; set; } = new();
}

public class AiAddFlightResponse
{
    public bool IsAddFlightRequest { get; set; } = true;
    public string? Message { get; set; }
    public string? FlightNumber { get; set; }
    public string? Airline { get; set; }
    public string? Destination { get; set; }
    public string? DepartureTime { get; set; }
    public string? ArrivalTime { get; set; }
    public string? Gate { get; set; }
    public string? Terminal { get; set; }
    public bool ArrivalEstimated { get; set; }
    public bool GateEstimated { get; set; }
    public bool TerminalEstimated { get; set; }
    public List<string> MissingRequiredFields { get; set; } = new();
}