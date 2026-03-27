using AFMS.Models;
using System.Collections.Generic;

namespace AFMS.Validators
{
    public class FlightBusinessRulesValidator
    {
        private readonly ILogger<FlightBusinessRulesValidator> _logger;

        public FlightBusinessRulesValidator(ILogger<FlightBusinessRulesValidator> logger)
        {
            _logger = logger;
        }

        public (bool IsValid, List<string> Errors) ValidateFlightCreation(Flight flight)
        {
            var errors = new List<string>();

            try
            {
                _logger.LogInformation("Validating flight creation rules for: {FlightNumber}", flight.FlightNumber);

                if (flight.DepartureTime >= flight.ArrivalTime)
                    errors.Add("Arrival time must be after departure time");

                if ((flight.ArrivalTime - flight.DepartureTime).TotalHours > 24)
                    errors.Add("Flight duration cannot exceed 24 hours");

                if (flight.Terminal < 1 || flight.Terminal > 5)
                    errors.Add("Terminal must be between 1 and 5");

                if (string.IsNullOrWhiteSpace(flight.FlightNumber) || flight.FlightNumber.Length < 3)
                    errors.Add("Flight number must be at least 3 characters");

                if (flight.DepartureTime < DateTime.Now)
                    errors.Add("Departure time cannot be in the past");

                return (errors.Count == 0, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating flight creation rules");
                throw;
            }
        }

        public (bool IsValid, List<string> Errors) ValidateFlightUpdate(Flight oldFlight, Flight newFlight)
        {
            var errors = new List<string>();

            try
            {
                _logger.LogInformation("Validating flight update for: {FlightNumber}", newFlight.FlightNumber);

                var creationValidation = ValidateFlightCreation(newFlight);
                if (!creationValidation.IsValid)
                    errors.AddRange(creationValidation.Errors);

                if (oldFlight.FlightNumber != newFlight.FlightNumber)
                    errors.Add("Flight number cannot be changed");

                return (errors.Count == 0, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating flight update");
                throw;
            }
        }
    }
}
