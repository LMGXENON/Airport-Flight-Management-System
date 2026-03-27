using AFMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AFMS.Services
{
    public interface IAuditLogService
    {
        Task LogFlightCreationAsync(Flight flight, string createdBy);
        Task LogFlightUpdateAsync(Flight flight, Dictionary<string, object> changes, string updatedBy);
        Task LogFlightDeletionAsync(Flight flight, string deletedBy);
        Task<List<FlightAuditLog>> GetAuditLogsForFlightAsync(int flightId);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(ApplicationDbContext context, ILogger<AuditLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogFlightCreationAsync(Flight flight, string createdBy)
        {
            try
            {
                _logger.LogInformation("Logging flight creation: {FlightNumber}", flight.FlightNumber);
                var auditLog = new FlightAuditLog
                {
                    FlightId = flight.Id,
                    ActionType = "Create",
                    ChangedFields = $"Created new flight: {flight.FlightNumber}",
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = createdBy
                };

                _context.Set<FlightAuditLog>().Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging flight creation");
                throw;
            }
        }

        public async Task LogFlightUpdateAsync(Flight flight, Dictionary<string, object> changes, string updatedBy)
        {
            try
            {
                _logger.LogInformation("Logging flight update: {FlightNumber}", flight.FlightNumber);
                var auditLog = new FlightAuditLog
                {
                    FlightId = flight.Id,
                    ActionType = "Update",
                    ChangedFields = string.Join(", ", changes.Keys),
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = updatedBy
                };

                _context.Set<FlightAuditLog>().Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging flight update");
                throw;
            }
        }

        public async Task LogFlightDeletionAsync(Flight flight, string deletedBy)
        {
            try
            {
                _logger.LogInformation("Logging flight deletion: {FlightNumber}", flight.FlightNumber);
                var auditLog = new FlightAuditLog
                {
                    FlightId = flight.Id,
                    ActionType = "Delete",
                    ChangedFields = $"Deleted flight: {flight.FlightNumber}",
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = deletedBy
                };

                _context.Set<FlightAuditLog>().Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging flight deletion");
                throw;
            }
        }

        public async Task<List<FlightAuditLog>> GetAuditLogsForFlightAsync(int flightId)
        {
            try
            {
                _logger.LogInformation("Retrieving audit logs for flight {FlightId}", flightId);
                return await Task.FromResult(
                    _context.Set<FlightAuditLog>()
                        .Where(a => a.FlightId == flightId)
                        .OrderByDescending(a => a.ChangedAt)
                        .ToList()
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                throw;
            }
        }
    }
}
