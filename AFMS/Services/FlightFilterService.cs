using AFMS.Models;
using System.Collections.Generic;
using System.Linq;

namespace AFMS.Services
{
    public class FlightFilterService
    {
        private readonly ILogger<FlightFilterService> _logger;

        public FlightFilterService(ILogger<FlightFilterService> logger)
        {
            _logger = logger;
        }

        public List<Flight> ApplyFilter(List<Flight> flights, FlightFilter filter)
        {
            try
            {
                _logger.LogInformation("Applying filter to {Count} flights", flights.Count);
                var result = flights;

                if (!string.IsNullOrWhiteSpace(filter.FlightNumber))
                    result = result.Where(f => f.FlightNumber.Contains(filter.FlightNumber, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!string.IsNullOrWhiteSpace(filter.Airline))
                    result = result.Where(f => f.Airline.Contains(filter.Airline, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!string.IsNullOrWhiteSpace(filter.Destination))
                    result = result.Where(f => f.Destination.Contains(filter.Destination, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!string.IsNullOrWhiteSpace(filter.Status))
                    result = result.Where(f => f.Status.Equals(filter.Status, StringComparison.OrdinalIgnoreCase)).ToList();

                if (filter.Terminal.HasValue)
                    result = result.Where(f => f.Terminal == filter.Terminal.Value).ToList();

                if (filter.DepartureDateFrom.HasValue)
                    result = result.Where(f => f.DepartureTime.Date >= filter.DepartureDateFrom.Value.Date).ToList();

                if (filter.DepartureDateTo.HasValue)
                    result = result.Where(f => f.DepartureTime.Date <= filter.DepartureDateTo.Value.Date).ToList();

                _logger.LogInformation("Filter returned {Count} flights", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying filter");
                throw;
            }
        }
    }
}
