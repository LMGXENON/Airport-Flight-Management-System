using AFMS.Data;
using AFMS.Hubs;
using AFMS.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AFMS.Services;

public class FlightSyncService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<FlightHub> _hubContext;
    private readonly ILogger<FlightSyncService> _logger;
    private readonly IConfiguration _configuration;

    public FlightSyncService(
        IServiceProvider serviceProvider,
        IHubContext<FlightHub> hubContext,
        ILogger<FlightSyncService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SyncFlightsAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var aeroDataBoxService = scope.ServiceProvider.GetRequiredService<AeroDataBoxService>();

            // Get flights from AeroDataBox for the next 24 hours
            var airportCode = _configuration["AeroDataBox:DefaultAirport"] ?? "EGLL";
            var dateFrom = DateTime.UtcNow;
            var dateTo = dateFrom.AddHours(24);

            var externalFlights = await aeroDataBoxService.GetAirportFlightsAsync(
                airportCode, 
                dateFrom, 
                dateTo, 
                withCancelled: true);

            var updatedFlights = new List<Flight>();
            var newFlights = new List<Flight>();

            foreach (var extFlight in externalFlights)
            {
                var flightNumber = extFlight.Number ?? "Unknown";
                var airline = extFlight.Airline?.Name ?? "Unknown Airline";
                
                // Parse times
                DateTime departureTime = DateTime.UtcNow;
                DateTime arrivalTime = DateTime.UtcNow;
                
                var departureLeg = extFlight.Departure;
                var arrivalLeg = extFlight.Arrival;
                
                if (departureLeg?.ScheduledTime?.Utc != null)
                {
                    DateTime.TryParse(departureLeg.ScheduledTime.Utc, out departureTime);
                }
                
                if (arrivalLeg?.ScheduledTime?.Utc != null)
                {
                    DateTime.TryParse(arrivalLeg.ScheduledTime.Utc, out arrivalTime);
                }

                var destination = extFlight.Direction == "Departure" 
                    ? (arrivalLeg?.Airport?.Iata ?? "Unknown")
                    : (departureLeg?.Airport?.Iata ?? "Unknown");

                var gate = extFlight.Direction == "Departure" 
                    ? departureLeg?.Gate 
                    : arrivalLeg?.Gate;

                var terminal = extFlight.Direction == "Departure" 
                    ? departureLeg?.Terminal ?? "1" 
                    : arrivalLeg?.Terminal ?? "1";

                var status = extFlight.Status ?? "Scheduled";

                // Check if flight exists in database
                var existingFlight = await context.Flights
                    .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber && 
                                            f.DepartureTime.Date == departureTime.Date);

                if (existingFlight != null)
                {
                    // Update existing flight
                    var hasChanged = false;

                    if (existingFlight.Status != status)
                    {
                        existingFlight.Status = status;
                        hasChanged = true;
                    }

                    if (existingFlight.Gate != gate && gate != null)
                    {
                        existingFlight.Gate = gate;
                        hasChanged = true;
                    }

                    if (existingFlight.DepartureTime != departureTime)
                    {
                        existingFlight.DepartureTime = departureTime;
                        hasChanged = true;
                    }

                    if (existingFlight.ArrivalTime != arrivalTime)
                    {
                        existingFlight.ArrivalTime = arrivalTime;
                        hasChanged = true;
                    }

                    if (hasChanged)
                    {
                        updatedFlights.Add(existingFlight);
                    }
                }
                else
                {
                    // Create new flight
                    var newFlight = new Flight
                    {
                        FlightNumber = flightNumber,
                        Airline = airline,
                        Destination = destination,
                        DepartureTime = departureTime,
                        ArrivalTime = arrivalTime,
                        Gate = gate,
                        Terminal = terminal,
                        Status = status
                    };

                    context.Flights.Add(newFlight);
                    newFlights.Add(newFlight);
                }
            }

            await context.SaveChangesAsync();

            // Notify connected clients about updates
            if (updatedFlights.Any())
            {
                _logger.LogInformation($"Sending {updatedFlights.Count} flight updates to clients");
                await _hubContext.Clients.Group("FlightUpdates")
                    .SendAsync("FlightUpdated", updatedFlights);
            }

            if (newFlights.Any())
            {
                _logger.LogInformation($"Sending {newFlights.Count} new flights to clients");
                await _hubContext.Clients.Group("FlightUpdates")
                    .SendAsync("FlightAdded", newFlights);
            }

            _logger.LogInformation($"Flight sync complete: {updatedFlights.Count} updated, {newFlights.Count} new");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing flights");
        }
    }
}
