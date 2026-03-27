using AFMS.Models;
using System.Threading.Tasks;

namespace AFMS.Services
{
    public class FlightExportService
    {
        private readonly ILogger<FlightExportService> _logger;

        public FlightExportService(ILogger<FlightExportService> logger)
        {
            _logger = logger;
        }

        public async Task<string> ExportFlightsToCSVAsync(List<Flight> flights)
        {
            try
            {
                _logger.LogInformation("Exporting {Count} flights to CSV", flights.Count);
                
                var csv = "FlightNumber,Airline,Destination,DepartureTime,ArrivalTime,Terminal,Status\n";
                
                foreach (var flight in flights)
                {
                    csv += $"\"{flight.FlightNumber}\",\"{flight.Airline}\",\"{flight.Destination}\"," +
                           $"\"{flight.DepartureTime:yyyy-MM-dd HH:mm:ss}\",\"{flight.ArrivalTime:yyyy-MM-dd HH:mm:ss}\"," +
                           $"{flight.Terminal},\"{flight.Status}\"\n";
                }

                return await Task.FromResult(csv);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting flights to CSV");
                throw;
            }
        }

        public async Task<string> ExportFlightsToJSONAsync(List<Flight> flights)
        {
            try
            {
                _logger.LogInformation("Exporting {Count} flights to JSON", flights.Count);
                
                var json = "[\n";
                for (int i = 0; i < flights.Count; i++)
                {
                    var flight = flights[i];
                    json += $"  {{\n" +
                            $"    \"flightNumber\": \"{flight.FlightNumber}\",\n" +
                            $"    \"airline\": \"{flight.Airline}\",\n" +
                            $"    \"destination\": \"{flight.Destination}\",\n" +
                            $"    \"departureTime\": \"{flight.DepartureTime:o}\",\n" +
                            $"    \"arrivalTime\": \"{flight.ArrivalTime:o}\",\n" +
                            $"    \"terminal\": {flight.Terminal},\n" +
                            $"    \"status\": \"{flight.Status}\"\n" +
                            $"  }}" + (i < flights.Count - 1 ? "," : "") + "\n";
                }
                json += "]";

                return await Task.FromResult(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting flights to JSON");
                throw;
            }
        }
    }
}
