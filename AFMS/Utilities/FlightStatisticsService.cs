using AFMS.Models;
using System.Collections.Generic;

namespace AFMS.Utilities
{
    public class FlightStatistics
    {
        public int TotalFlights { get; set; }
        public int ScheduledFlights { get; set; }
        public int DelayedFlights { get; set; }
        public int CancelledFlights { get; set; }
        public int CompletedFlights { get; set; }
        public double AverageFlightDuration { get; set; }
        public Dictionary<string, int> FlightsByTerminal { get; set; }
        public Dictionary<string, int> FlightsByAirline { get; set; }

        public FlightStatistics()
        {
            FlightsByTerminal = new Dictionary<string, int>();
            FlightsByAirline = new Dictionary<string, int>();
        }
    }

    public class FlightStatisticsService
    {
        private readonly ILogger<FlightStatisticsService> _logger;

        public FlightStatisticsService(ILogger<FlightStatisticsService> logger)
        {
            _logger = logger;
        }

        public FlightStatistics CalculateStatistics(List<Flight> flights)
        {
            try
            {
                _logger.LogInformation("Calculating statistics for {Count} flights", flights.Count);
                
                var stats = new FlightStatistics
                {
                    TotalFlights = flights.Count,
                    ScheduledFlights = flights.Count(f => f.Status == "Scheduled"),
                    DelayedFlights = flights.Count(f => f.Status == "Delayed"),
                    CancelledFlights = flights.Count(f => f.Status == "Cancelled"),
                    CompletedFlights = flights.Count(f => f.Status == "Completed")
                };

                if (flights.Count > 0)
                {
                    stats.AverageFlightDuration = flights
                        .Average(f => (f.ArrivalTime - f.DepartureTime).TotalHours);
                }

                for (int i = 1; i <= 5; i++)
                {
                    stats.FlightsByTerminal[$"Terminal {i}"] = flights.Count(f => f.Terminal == i);
                }

                foreach (var airline in flights.Select(f => f.Airline).Distinct())
                {
                    stats.FlightsByAirline[airline] = flights.Count(f => f.Airline == airline);
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating statistics");
                throw;
            }
        }
    }
}
