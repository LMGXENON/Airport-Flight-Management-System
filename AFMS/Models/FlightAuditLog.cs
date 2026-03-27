namespace AFMS.Models
{
    public interface IFlightAuditLog
    {
        int Id { get; set; }
        int FlightId { get; set; }
        string ActionType { get; set; }
        string ChangedFields { get; set; }
        DateTime ChangedAt { get; set; }
        string ChangedBy { get; set; }
    }

    public class FlightAuditLog : IFlightAuditLog
    {
        public int Id { get; set; }
        public int FlightId { get; set; }
        public string ActionType { get; set; } // Create, Update, Delete
        public string ChangedFields { get; set; } // JSON serialized changes
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; }
    }
}
