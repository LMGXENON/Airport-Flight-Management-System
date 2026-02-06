
using System.ComponentModel.DataAnnotations;

namespace AFMS.Models
{
    public class Flight
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
    }
}


