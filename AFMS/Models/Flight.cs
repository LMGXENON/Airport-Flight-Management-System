
using System.ComponentModel.DataAnnotations;

namespace AFMS.Models
{
    public class Flight : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Flight number is required")]
        [Display(Name = "Flight Number")]
        public string FlightNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Airline is required")]
        public string Airline { get; set; } = string.Empty;

        [Required(ErrorMessage = "Destination is required")]
        public string Destination { get; set; } = string.Empty;

        [Required(ErrorMessage = "Departure time is required")]
        [Display(Name = "Departure Time")]
        public DateTime DepartureTime { get; set; }

        [Required(ErrorMessage = "Arrival time is required")]
        [Display(Name = "Arrival Time")]
        public DateTime ArrivalTime { get; set; }

        [Display(Name = "Gate")]
        public string? Gate { get; set; }

        [Display(Name = "Terminal")]
        public string Terminal { get; set; } = "1";

        public string Status { get; set; } = "Scheduled";

        /// <summary>
        /// Custom validation for complex business rules
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validate that arrival time is after departure time
            if (ArrivalTime <= DepartureTime)
            {
                yield return new ValidationResult(
                    "Arrival time must be after departure time",
                    new[] { nameof(ArrivalTime) });
            }

            // Validate that flight duration does not exceed 24 hours
            var duration = ArrivalTime - DepartureTime;
            if (duration.TotalHours > 24)
            {
                yield return new ValidationResult(
                    "Flight duration cannot exceed 24 hours",
                    new[] { nameof(ArrivalTime), nameof(DepartureTime) });
            }

            // Validate terminal is between 1 and 5
            if (!int.TryParse(Terminal, out int terminalNum) || terminalNum < 1 || terminalNum > 5)
            {
                yield return new ValidationResult(
                    "Terminal must be a number between 1 and 5",
                    new[] { nameof(Terminal) });
            }

            // Validate flight number format (at least 3 characters, no special chars)
            if (string.IsNullOrEmpty(FlightNumber) || FlightNumber.Length < 3 || !FlightNumber.All(c => char.IsLetterOrDigit(c)))
            {
                yield return new ValidationResult(
                    "Flight number must be at least 3 alphanumeric characters",
                    new[] { nameof(FlightNumber) });
            }

            // Validate status is valid
            var validStatuses = new[] { "Scheduled", "Delayed", "Cancelled", "In Flight", "Landed" };
            if (!validStatuses.Contains(Status))
            {
                yield return new ValidationResult(
                    $"Status must be one of: {string.Join(", ", validStatuses)}",
                    new[] { nameof(Status) });
            }
        }
    }
}


