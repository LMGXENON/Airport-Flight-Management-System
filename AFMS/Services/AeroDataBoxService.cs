using System.Text.Json;
using AFMS.Models;

namespace AFMS.Services;

public class AeroDataBoxService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AeroDataBoxService> _logger;

    public AeroDataBoxService(HttpClient httpClient, IConfiguration configuration, ILogger<AeroDataBoxService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
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
                return new List<AeroDataBoxFlight>(); // Return empty list instead of mock data
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

            _logger.LogInformation($"Total flights fetched: {flights.Count}");
            return flights; // Return actual data, even if empty
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching flights from AeroDataBox API");
            return new List<AeroDataBoxFlight>(); // Return empty list on error
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

        var requestUrl = $"https://{apiHost}/flights/airports/icao/{airportCode}/{formattedFrom}/{formattedTo}?withLeg=true&direction=Both&withCancelled={withCancelled.ToString().ToLowerInvariant()}&withCodeshared=true&withCargo=false&withPrivate=false&withLocation=false";
        _logger.LogInformation($"API Request URL: {requestUrl}");

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
            _logger.LogWarning($"AeroDataBox API returned status code: {response.StatusCode}, Body: {errorContent}");
            return new List<AeroDataBoxFlight>();
        }

        var body = await response.Content.ReadAsStringAsync();
        _logger.LogInformation($"API Response: {body.Substring(0, Math.Min(500, body.Length))}...");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };

        var data = JsonSerializer.Deserialize<AeroDataBoxResponse>(body, options);
        var flights = new List<AeroDataBoxFlight>();

        if (data?.Departures != null)
        {
            _logger.LogInformation($"Fetched {data.Departures.Count} departures from API");
            foreach (var f in data.Departures) f.Direction = "Departure";
            flights.AddRange(data.Departures);
        }

        if (data?.Arrivals != null)
        {
            _logger.LogInformation($"Fetched {data.Arrivals.Count} arrivals from API");
            foreach (var f in data.Arrivals) f.Direction = "Arrival";
            flights.AddRange(data.Arrivals);
        }

        return flights;
    }

    private List<AeroDataBoxFlight> GetMockFlights()
    {
        // Return mock data as fallback
        return new List<AeroDataBoxFlight>
        {
            new AeroDataBoxFlight
            {
                Number = "BA 1234",
                Direction = "Departure",
                Airline = new Airline { Name = "British Airways" },
                Departure = new FlightMovement
                {
                    Airport = new Airport { Iata = "LHR", Name = "London Heathrow" },
                    ScheduledTime = new ScheduledTime { Local = DateTime.Now.AddHours(2).ToString("o") },
                    Gate = "B22",
                    Terminal = "5"
                },
                Arrival = new FlightMovement
                {
                    Airport = new Airport { Iata = "JFK", Name = "New York JFK" },
                    ScheduledTime = new ScheduledTime { Local = DateTime.Now.AddHours(10).ToString("o") }
                },
                Status = "Expected"
            },
            new AeroDataBoxFlight
            {
                Number = "BA 5678",
                Direction = "Departure",
                Airline = new Airline { Name = "British Airways" },
                Departure = new FlightMovement
                {
                    Airport = new Airport { Iata = "LHR", Name = "London Heathrow" },
                    ScheduledTime = new ScheduledTime { Local = DateTime.Now.AddHours(1).ToString("o") },
                    Gate = "A15",
                    Terminal = "5"
                },
                Arrival = new FlightMovement
                {
                    Airport = new Airport { Iata = "DXB", Name = "Dubai" },
                    ScheduledTime = new ScheduledTime { Local = DateTime.Now.AddHours(7).ToString("o") }
                },
                Status = "Boarding"
            },
            new AeroDataBoxFlight
            {
                Number = "BA 9012",
                Direction = "Departure",
                Airline = new Airline { Name = "British Airways" },
                Departure = new FlightMovement
                {
                    Airport = new Airport { Iata = "LHR", Name = "London Heathrow" },
                    ScheduledTime = new ScheduledTime { Local = DateTime.Now.AddHours(3).ToString("o") },
                    Gate = "C8",
                    Terminal = "5"
                },
                Arrival = new FlightMovement
                {
                    Airport = new Airport { Iata = "DXB", Name = "Dubai" },
                    ScheduledTime = new ScheduledTime { Local = DateTime.Now.AddHours(4).ToString("o") }
                },
                Status = "Delayed"
            },
            new AeroDataBoxFlight
            {
                Number = "BA 3456",
                Direction = "Departure",
                Airline = new Airline { Name = "British Airways" },
                Departure = new FlightMovement
                {
                    Airport = new Airport { Iata = "LHR", Name = "London Heathrow" },
                    ScheduledTime = new ScheduledTime { Local = DateTime.Now.AddHours(5).ToString("o") },
                    Gate = "D12",
                    Terminal = "5"
                },
                Arrival = new FlightMovement
                {
                    Airport = new Airport { Iata = "SYD", Name = "Sydney" },
                    ScheduledTime = new ScheduledTime { Local = DateTime.Now.AddHours(22).ToString("o") }
                },
                Status = "Departed"
            }
        };
    }
}
