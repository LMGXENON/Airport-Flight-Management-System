namespace AFMS.Repositories
{
    /// <summary>
    /// Repository pattern interface for Flight data access
    /// Provides abstraction for database operations
    /// </summary>
    public interface IFlightRepository
    {
        Task<Flight> GetByIdAsync(int id);
        Task<List<Flight>> GetAllAsync();
        Task<Flight> CreateAsync(Flight flight);
        Task<Flight> UpdateAsync(Flight flight);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task SaveChangesAsync();
    }

    public class FlightRepository : IFlightRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FlightRepository> _logger;

        public FlightRepository(ApplicationDbContext context, ILogger<FlightRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Flight> GetByIdAsync(int id)
        {
            _logger.LogInformation($"Getting flight with ID: {id}");
            return await _context.Flights.FindAsync(id);
        }

        public async Task<List<Flight>> GetAllAsync()
        {
            _logger.LogInformation("Getting all flights");
            return await _context.Flights.ToListAsync();
        }

        public async Task<Flight> CreateAsync(Flight flight)
        {
            _logger.LogInformation($"Creating new flight: {flight.FlightNumber}");
            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();
            return flight;
        }

        public async Task<Flight> UpdateAsync(Flight flight)
        {
            _logger.LogInformation($"Updating flight: {flight.FlightNumber}");
            _context.Flights.Update(flight);
            await _context.SaveChangesAsync();
            return flight;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation($"Deleting flight with ID: {id}");
            var flight = await GetByIdAsync(id);
            if (flight == null)
                return false;

            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Flights.AnyAsync(f => f.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
