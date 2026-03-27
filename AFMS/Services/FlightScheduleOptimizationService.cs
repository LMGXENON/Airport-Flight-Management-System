using AFMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AFMS.Services
{
    public class FlightScheduleOptimizationService
    {
        private readonly ILogger<FlightScheduleOptimizationService> _logger;

        public FlightScheduleOptimizationService(ILogger<FlightScheduleOptimizationService> logger)
        {
            _logger = logger;
        }

        public List<Flight> GetFlightsByTerminal(List<Flight> flights, int terminal)
        {
            try
            {
                _logger.LogInformation("Retrieving flights for terminal {Terminal}", terminal);
                return flights.Where(f => f.Terminal == terminal).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flights by terminal");
                throw;
            }
        }

        public List<Flight> GetFlightsByHour(List<Flight> flights, int hour)
        {
            try
            {
                _logger.LogInformation("Retrieving flights departing in hour {Hour}", hour);
                return flights.Where(f => f.DepartureTime.Hour == hour).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flights by hour");
                throw;
            }
        }

        public Dictionary<int, int> GetTerminalUtilization(List<Flight> flights)
        {
            try
            {
                _logger.LogInformation("Calculating terminal utilization");
                var utilization = new Dictionary<int, int>();
                
                for (int i = 1; i <= 5; i++)
                {
                    utilization[i] = flights.Count(f => f.Terminal == i);
                }

                return utilization;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating terminal utilization");
                throw;
            }
        }
    }
}
