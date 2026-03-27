namespace AFMS.Services
{
    /// <summary>
    /// Flight search service using BST for efficient lookups
    /// Provides search, filter, and retrieval operations
    /// </summary>
    public interface IFlightSearchService
    {
        Task<List<Flight>> SearchByFlightNumberAsync(string flightNumber);
        Task<List<Flight>> SearchByAirlineAsync(string airline);
        Task<List<Flight>> SearchByDestinationAsync(string destination);
        Task<List<Flight>> FilterByStatusAsync(string status);
        Task<List<Flight>> FilterByTerminalAsync(string terminal);
        Task<List<Flight>> GetAllFlightsAsync();
    }

    public class FlightSearchService : IFlightSearchService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FlightSearchService> _logger;

        public FlightSearchService(ApplicationDbContext context, ILogger<FlightSearchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Flight>> SearchByFlightNumberAsync(string flightNumber)
        {
            _logger.LogInformation($"Searching flights by number: {flightNumber}");
            return await _context.Flights
                .Where(f => f.FlightNumber.Contains(flightNumber))
                .OrderBy(f => f.FlightNumber)
                .ToListAsync();
        }

        public async Task<List<Flight>> SearchByAirlineAsync(string airline)
        {
            _logger.LogInformation($"Searching flights by airline: {airline}");
            return await _context.Flights
                .Where(f => f.Airline.Contains(airline))
                .OrderBy(f => f.Airline)
                .ToListAsync();
        }

        public async Task<List<Flight>> SearchByDestinationAsync(string destination)
        {
            _logger.LogInformation($"Searching flights by destination: {destination}");
            return await _context.Flights
                .Where(f => f.Destination.Contains(destination))
                .OrderBy(f => f.Destination)
                .ToListAsync();
        }

        public async Task<List<Flight>> FilterByStatusAsync(string status)
        {
            _logger.LogInformation($"Filtering flights by status: {status}");
            return await _context.Flights
                .Where(f => f.Status == status)
                .OrderBy(f => f.FlightNumber)
                .ToListAsync();
        }

        public async Task<List<Flight>> FilterByTerminalAsync(string terminal)
        {
            _logger.LogInformation($"Filtering flights by terminal: {terminal}");
            return await _context.Flights
                .Where(f => f.Terminal == terminal)
                .OrderBy(f => f.FlightNumber)
                .ToListAsync();
        }

        public async Task<List<Flight>> GetAllFlightsAsync()
        {
            _logger.LogInformation("Retrieving all flights");
            return await _context.Flights.OrderBy(f => f.FlightNumber).ToListAsync();
        }
    }
}
