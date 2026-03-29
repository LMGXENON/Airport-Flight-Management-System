using AFMS.Data;
using AFMS.Hubs;
using AFMS.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
                var now = DateTime.UtcNow;
                DateTime departureTime = now;
                DateTime arrivalTime = now;

                var departureLeg = extFlight.Departure;
                var arrivalLeg = extFlight.Arrival;

                departureTime = ParseUtcOrFallback(departureLeg?.ScheduledTime?.Utc, departureTime);
                arrivalTime = ParseUtcOrFallback(arrivalLeg?.ScheduledTime?.Utc, arrivalTime);

                var destination = extFlight.Direction == "Departure" 
                    ? (arrivalLeg?.Airport?.Iata ?? "Unknown")
                    : (departureLeg?.Airport?.Iata ?? "Unknown");

                var gate = extFlight.Direction == "Departure" 
                    ? departureLeg?.Gate 
                    : arrivalLeg?.Gate;

                var terminal = extFlight.Direction == "Departure" 
                    ? departureLeg?.Terminal ?? "1" 
                    : arrivalLeg?.Terminal ?? "1";

                var aircraftType = extFlight.Aircraft?.Model;

                var status = FlightStatusCatalog.Normalize(extFlight.Status);

                // Match by flight number + departure day to avoid duplicates across daily refreshes.
                var departureDateStart = departureTime.Date;
                var departureDateEnd = departureDateStart.AddDays(1);

                var existingFlight = await context.Flights
                    .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber &&
                                            f.DepartureTime >= departureDateStart &&
                                            f.DepartureTime < departureDateEnd);

                if (existingFlight != null)
                {
                    // Don't overwrite flights that have been manually edited by a user
                    if (existingFlight.IsManualEntry)
                        continue;

                    // Track a change flag so we only broadcast flights that actually changed.
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

                    if (!string.IsNullOrWhiteSpace(aircraftType) &&
                        !string.Equals(existingFlight.AircraftType, aircraftType, StringComparison.OrdinalIgnoreCase))
                    {
                        existingFlight.AircraftType = aircraftType;
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
                        AircraftType = aircraftType,
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
                _logger.LogInformation("Sending {UpdatedFlightCount} flight updates to clients", updatedFlights.Count);
                // Push deltas only, so connected dashboards can patch rows in place.
                await _hubContext.Clients.Group("FlightUpdates")
                    .SendAsync("FlightUpdated", updatedFlights);
            }

            if (newFlights.Any())
            {
                _logger.LogInformation("Sending {NewFlightCount} new flights to clients", newFlights.Count);
                await _hubContext.Clients.Group("FlightUpdates")
                    .SendAsync("FlightAdded", newFlights);
            }

            _logger.LogInformation("Flight sync complete: {UpdatedFlightCount} updated, {NewFlightCount} new", updatedFlights.Count, newFlights.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing flights");
        }
    }

    private static DateTime ParseUtcOrFallback(string? utcValue, DateTime fallback)
    {
        if (string.IsNullOrWhiteSpace(utcValue))
            return fallback;

        return DateTimeOffset.TryParse(
                utcValue,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsedUtc)
            ? parsedUtc.UtcDateTime
            : fallback;
    }
}
