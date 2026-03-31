
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AFMS.Models
{
    [Index(nameof(FlightNumber), nameof(DepartureTime))]
    public class Flight : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Flight number is required")]
        [Display(Name = "Flight Number")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Flight number must be between 2 and 10 characters")]
        [RegularExpression(@"^[A-Za-z0-9-]+$", ErrorMessage = "Flight number can only contain letters, numbers, and hyphen")]
        public string FlightNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Airline is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Airline must be between 2 and 100 characters")]
        public string Airline { get; set; } = string.Empty;

        [Required(ErrorMessage = "Destination is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Destination must be between 2 and 100 characters")]
        public string Destination { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Origin must be 100 characters or fewer")]
        [Display(Name = "Origin")]
        public string? Origin { get; set; }

        [Required(ErrorMessage = "Departure time is required")]
        [Display(Name = "Departure Time")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DepartureTime { get; set; }

        [Required(ErrorMessage = "Arrival time is required")]
        [Display(Name = "Arrival Time")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime ArrivalTime { get; set; }

        [Display(Name = "Gate")]
        [StringLength(10, ErrorMessage = "Gate must be 10 characters or fewer")]
        [RegularExpression(@"^[A-Za-z0-9-]*$", ErrorMessage = "Gate can only contain letters, numbers, and hyphen")]
        public string? Gate { get; set; }

        [Display(Name = "Terminal")]
        [Required(ErrorMessage = "Terminal is required")]
        [RegularExpression(@"^[1-5]$", ErrorMessage = "Terminal must be 1, 2, 3, 4, or 5")]
        public string Terminal { get; set; } = "1";

        [Display(Name = "Aircraft Type")]
        [StringLength(80, ErrorMessage = "Aircraft type must be 80 characters or fewer")]
        public string? AircraftType { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(20, ErrorMessage = "Status must be 20 characters or fewer")]
        public string Status { get; set; } = "Scheduled";

        /// <summary>True when this flight was added or edited manually via the UI.
        /// The background sync service will not overwrite fields on manual entries.</summary>
        public bool IsManualEntry { get; set; } = false;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ArrivalTime <= DepartureTime)
            {
                yield return new ValidationResult(
                    "Arrival time must be later than departure time.",
                    [nameof(ArrivalTime), nameof(DepartureTime)]);
            }

            if ((ArrivalTime - DepartureTime).TotalHours > 24)
            {
                yield return new ValidationResult(
                    "Flight duration cannot exceed 24 hours for manual entries.",
                    [nameof(ArrivalTime), nameof(DepartureTime)]);
            }
        }
    }
}


