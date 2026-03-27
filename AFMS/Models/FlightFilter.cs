namespace AFMS.Models
{
    public class FlightFilter
    {
        public string FlightNumber { get; set; }
        public string Airline { get; set; }
        public string Destination { get; set; }
        public string Status { get; set; }
        public int? Terminal { get; set; }
        public DateTime? DepartureDateFrom { get; set; }
        public DateTime? DepartureDateTo { get; set; }

        public FlightFilter() { }

        public FlightFilter(
            string flightNumber = null,
            string airline = null,
            string destination = null,
            string status = null,
            int? terminal = null,
            DateTime? departureDateFrom = null,
            DateTime? departureDateTo = null)
        {
            FlightNumber = flightNumber;
            Airline = airline;
            Destination = destination;
            Status = status;
            Terminal = terminal;
            DepartureDateFrom = departureDateFrom;
            DepartureDateTo = departureDateTo;
        }

        public bool HasFilters =>
            !string.IsNullOrWhiteSpace(FlightNumber) ||
            !string.IsNullOrWhiteSpace(Airline) ||
            !string.IsNullOrWhiteSpace(Destination) ||
            !string.IsNullOrWhiteSpace(Status) ||
            Terminal.HasValue ||
            DepartureDateFrom.HasValue ||
            DepartureDateTo.HasValue;
    }
}
