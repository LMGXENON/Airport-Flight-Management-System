using AFMS.Models;
using System.Threading.Tasks;

namespace AFMS.Services
{
    public class FlightDuplicationCheckService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FlightDuplicationCheckService> _logger;

        public FlightDuplicationCheckService(ApplicationDbContext context, ILogger<FlightDuplicationCheckService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> IsDuplicateFlightAsync(string flightNumber, DateTime departureTime)
        {
            try
            {
                _logger.LogInformation("Checking for duplicate flight: {FlightNumber}", flightNumber);
                
                var existingFlight = _context.Flights
                    .Where(f => f.FlightNumber == flightNumber && 
                                f.DepartureTime.Date == departureTime.Date)
                    .FirstOrDefault();

                return existingFlight != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicate flight");
                throw;
            }
        }

        public async Task<List<Flight>> FindDuplicatesAsync(Flight flight)
        {
            try
            {
                _logger.LogInformation("Finding potential duplicates for: {FlightNumber}", flight.FlightNumber);
                
                var duplicates = _context.Flights
                    .Where(f => f.FlightNumber == flight.FlightNumber && 
                                f.DepartureTime.Date == flight.DepartureTime.Date &&
                                f.Airline == flight.Airline)
                    .ToList();

                return duplicates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding duplicate flights");
                throw;
            }
        }
    }
}
