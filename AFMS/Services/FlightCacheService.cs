using AFMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AFMS.Services
{
    public interface IFlightCacheService
    {
        Task<Flight> GetFlightByIdAsync(int id);
        Task<List<Flight>> GetAllFlightsAsync();
        Task SetFlightAsync(Flight flight);
        Task InvalidateFlightAsync(int id);
        Task InvalidateAllFlightsAsync();
    }

    public class FlightCacheService : IFlightCacheService
    {
        private readonly ILogger<FlightCacheService> _logger;
        private readonly Dictionary<int, Flight> _cache = new();
        private readonly object _lockObject = new();

        public FlightCacheService(ILogger<FlightCacheService> logger)
        {
            _logger = logger;
        }

        public async Task<Flight> GetFlightByIdAsync(int id)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_cache.ContainsKey(id))
                    {
                        _logger.LogInformation("Flight {FlightId} found in cache", id);
                        return _cache[id];
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flight from cache");
                throw;
            }
        }

        public async Task<List<Flight>> GetAllFlightsAsync()
        {
            try
            {
                lock (_lockObject)
                {
                    return new List<Flight>(_cache.Values);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all flights from cache");
                throw;
            }
        }

        public async Task SetFlightAsync(Flight flight)
        {
            try
            {
                _logger.LogInformation("Caching flight {FlightId}", flight.Id);
                lock (_lockObject)
                {
                    _cache[flight.Id] = flight;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching flight");
                throw;
            }
        }

        public async Task InvalidateFlightAsync(int id)
        {
            try
            {
                _logger.LogInformation("Invalidating flight {FlightId} from cache", id);
                lock (_lockObject)
                {
                    _cache.Remove(id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating flight cache");
                throw;
            }
        }

        public async Task InvalidateAllFlightsAsync()
        {
            try
            {
                _logger.LogInformation("Invalidating all flights from cache");
                lock (_lockObject)
                {
                    _cache.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating all flight caches");
                throw;
            }
        }
    }
}
