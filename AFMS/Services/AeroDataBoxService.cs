using System.Net;
using System.Text.Json;
using AFMS.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AFMS.Services;

public class AeroDataBoxService
{
    public const string ApiErrorCacheKey = "aerodatabox_api_error";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AeroDataBoxService> _logger;
    private readonly IMemoryCache _cache;

    public AeroDataBoxService(HttpClient httpClient, IConfiguration configuration, ILogger<AeroDataBoxService> logger, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
    }

    public async Task<List<AeroDataBoxFlight>> GetAirportFlightsAsync(string airportCode, DateTime date)
    {
        return await GetAirportFlightsAsync(airportCode, date, date.AddHours(12), withCancelled: false);
    }

    public async Task<List<AeroDataBoxFlight>> GetAirportFlightsAsync(string airportCode, DateTime dateFrom, DateTime dateTo, bool withCancelled)
    {
        try
        {
            var apiKey = _configuration["AeroDataBox:ApiKey"];
            var apiHost = _configuration["AeroDataBox:ApiHost"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiHost))
            {
                _logger.LogWarning("AeroDataBox API credentials not configured");
                return []; // Return empty list instead of mock data
            }

            if (dateTo < dateFrom)
            {
                (dateFrom, dateTo) = (dateTo, dateFrom);
            }
            var flights = new List<AeroDataBoxFlight>();

            var maxWindow = TimeSpan.FromHours(12);
            var currentFrom = dateFrom;

            while (currentFrom <= dateTo)
            {
                var currentTo = currentFrom.Add(maxWindow);
                if (currentTo > dateTo)
                {
                    currentTo = dateTo;
                }

                var windowFlights = await FetchAirportFlightsWindowAsync(
                    airportCode,
                    currentFrom,
                    currentTo,
                    withCancelled,
                    apiKey,
                    apiHost);

                flights.AddRange(windowFlights);

                if (currentTo >= dateTo)
                {
                    break;
                }

                currentFrom = currentTo.AddMinutes(1);
            }

            // Sort by scheduled time to interleave departures and arrivals chronologically
            flights = flights.OrderBy(f => {
                var leg = f.Direction == "Departure" ? f.Departure : f.Arrival;
                if (DateTime.TryParse(leg?.ScheduledTime?.Utc, out var dt)) return dt;
                return DateTime.MaxValue;
            }).ToList();

            _logger.LogInformation("Total flights fetched: {FlightCount}", flights.Count);
            return flights; // Return actual data, even if empty
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching flights from AeroDataBox API");
            return []; // Return empty list on error
        }
    }

    private async Task<List<AeroDataBoxFlight>> FetchAirportFlightsWindowAsync(
        string airportCode,
        DateTime dateFrom,
        DateTime dateTo,
        bool withCancelled,
        string apiKey,
        string apiHost)
    {
        var formattedFrom = dateFrom.ToString("yyyy-MM-dd'T'HH:mm");
        var formattedTo = dateTo.ToString("yyyy-MM-dd'T'HH:mm");

        var requestUrl = $"https://{apiHost}/flights/airports/icao/{airportCode}/{formattedFrom}/{formattedTo}?withLeg=true&direction=Both&withCancelled={withCancelled.ToString().ToLowerInvariant()}&withCodeshared=false&withCargo=false&withPrivate=false&withLocation=false";
        _logger.LogInformation("API request URL: {RequestUrl}", requestUrl);

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(requestUrl),
            Headers =
            {
                { "x-rapidapi-key", apiKey },
                { "x-rapidapi-host", apiHost },
            },
        };

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("AeroDataBox API returned status code: {StatusCode}, Body: {ErrorBody}", response.StatusCode, errorContent);

            var errorReason = response.StatusCode == HttpStatusCode.TooManyRequests
                ? "API quota exceeded — you have used your monthly or per-second limit on the BASIC plan. Upgrade at rapidapi.com or wait for your quota to reset."
                : $"API error ({(int)response.StatusCode} {response.StatusCode})";
            _cache.Set(ApiErrorCacheKey, errorReason, TimeSpan.FromMinutes(5));

            return [];
        }

        // Clear any previous error on success
        _cache.Remove(ApiErrorCacheKey);

        var body = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("API response preview: {BodyPreview}...", body[..Math.Min(500, body.Length)]);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };

        var data = JsonSerializer.Deserialize<AeroDataBoxResponse>(body, options);
        var flights = new List<AeroDataBoxFlight>();

        if (data?.Departures != null)
        {
            _logger.LogInformation("Fetched {DepartureCount} departures from API", data.Departures.Count);
            foreach (var f in data.Departures) f.Direction = "Departure";
            flights.AddRange(data.Departures);
        }

        if (data?.Arrivals != null)
        {
            _logger.LogInformation("Fetched {ArrivalCount} arrivals from API", data.Arrivals.Count);
            foreach (var f in data.Arrivals) f.Direction = "Arrival";
            flights.AddRange(data.Arrivals);
        }

        return flights;
    }

}
