using AFMS.Models;
using System.Collections.Generic;

namespace AFMS.Utilities
{
    public class FlightComparisonService
    {
        private readonly ILogger<FlightComparisonService> _logger;

        public FlightComparisonService(ILogger<FlightComparisonService> logger)
        {
            _logger = logger;
        }

        public List<string> CompareTwoFlights(Flight flight1, Flight flight2)
        {
            try
            {
                _logger.LogInformation("Comparing flights {F1} and {F2}", flight1.FlightNumber, flight2.FlightNumber);
                var differences = new List<string>();

                if (flight1.FlightNumber != flight2.FlightNumber)
                    differences.Add($"Flight Number: {flight1.FlightNumber} vs {flight2.FlightNumber}");

                if (flight1.Airline != flight2.Airline)
                    differences.Add($"Airline: {flight1.Airline} vs {flight2.Airline}");

                if (flight1.Destination != flight2.Destination)
                    differences.Add($"Destination: {flight1.Destination} vs {flight2.Destination}");

                if (flight1.DepartureTime != flight2.DepartureTime)
                    differences.Add($"Departure Time: {flight1.DepartureTime} vs {flight2.DepartureTime}");

                if (flight1.ArrivalTime != flight2.ArrivalTime)
                    differences.Add($"Arrival Time: {flight1.ArrivalTime} vs {flight2.ArrivalTime}");

                if (flight1.Terminal != flight2.Terminal)
                    differences.Add($"Terminal: {flight1.Terminal} vs {flight2.Terminal}");

                if (flight1.Status != flight2.Status)
                    differences.Add($"Status: {flight1.Status} vs {flight2.Status}");

                return differences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing flights");
                throw;
            }
        }

        public bool AreFlightsIdentical(Flight flight1, Flight flight2)
        {
            return CompareTwoFlights(flight1, flight2).Count == 0;
        }
    }
}
