using System.ComponentModel.DataAnnotations;

namespace AFMS.Models
{
    /// <summary>
    /// Custom validation class for Flight entities
    /// Handles complex validation logic beyond simple attributes
    /// </summary>
    public class FlightValidation : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var flight = validationContext.ObjectInstance as Flight;
            if (flight == null)
                yield break;

            // Validate that arrival time is after departure time
            if (flight.ArrivalTime <= flight.DepartureTime)
            {
                yield return new ValidationResult(
                    "Arrival time must be after departure time",
                    new[] { nameof(Flight.ArrivalTime) });
            }

            // Validate that flight duration does not exceed 24 hours
            var duration = flight.ArrivalTime - flight.DepartureTime;
            if (duration.TotalHours > 24)
            {
                yield return new ValidationResult(
                    "Flight duration cannot exceed 24 hours",
                    new[] { nameof(Flight.ArrivalTime), nameof(Flight.DepartureTime) });
            }

            // Validate terminal is between 1 and 5
            if (!int.TryParse(flight.Terminal, out int terminal) || terminal < 1 || terminal > 5)
            {
                yield return new ValidationResult(
                    "Terminal must be a number between 1 and 5",
                    new[] { nameof(Flight.Terminal) });
            }

            // Validate flight number format
            if (string.IsNullOrEmpty(flight.FlightNumber) || flight.FlightNumber.Length < 3)
            {
                yield return new ValidationResult(
                    "Flight number must be at least 3 characters long",
                    new[] { nameof(Flight.FlightNumber) });
            }

            // Validate airline name is not empty
            if (string.IsNullOrWhiteSpace(flight.Airline))
            {
                yield return new ValidationResult(
                    "Airline name is required",
                    new[] { nameof(Flight.Airline) });
            }

            // Validate destination is not empty
            if (string.IsNullOrWhiteSpace(flight.Destination))
            {
                yield return new ValidationResult(
                    "Destination is required",
                    new[] { nameof(Flight.Destination) });
            }

            // Validate status is valid
            var validStatuses = new[] { "Scheduled", "Delayed", "Cancelled", "In Flight", "Landed" };
            if (!validStatuses.Contains(flight.Status))
            {
                yield return new ValidationResult(
                    $"Status must be one of: {string.Join(", ", validStatuses)}",
                    new[] { nameof(Flight.Status) });
            }
        }
    }
}
