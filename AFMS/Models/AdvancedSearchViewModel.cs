namespace AFMS.Models;

using System.ComponentModel.DataAnnotations;

public class AdvancedSearchViewModel
{
    // --- Filter properties ---
    [StringLength(10, ErrorMessage = "Flight must be 10 characters or fewer")]
    public string? Flight { get; set; }

    [StringLength(100, ErrorMessage = "Airline must be 100 characters or fewer")]
    public string? Airline { get; set; }

    [StringLength(100, ErrorMessage = "Origin must be 100 characters or fewer")]
    public string? Origin { get; set; }

    [StringLength(100, ErrorMessage = "Destination must be 100 characters or fewer")]
    public string? Destination { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DepartureDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ArrivalDate { get; set; }

    [RegularExpression(@"^[1-5]$", ErrorMessage = "Terminal must be between 1 and 5")]
    public string? Terminal { get; set; }

    public List<string> Statuses { get; set; } = new();

    /// <summary>"Departure", "Arrival", or null for both.</summary>
    [RegularExpression(@"^(Departure|Arrival)?$", ErrorMessage = "Direction must be 'Departure', 'Arrival', or empty")]
    public string? Direction { get; set; }

    /// <summary>Earliest scheduled time to include, formatted as "HH:mm".</summary>
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Time must be in HH:mm format")]
    public string? TimeRangeStart { get; set; }

    /// <summary>Latest scheduled time to include, formatted as "HH:mm".</summary>
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Time must be in HH:mm format")]
    public string? TimeRangeEnd { get; set; }

    // --- Sorting ---
    /// <summary>Column to sort by: flight | airline | departure | arrival | status.</summary>
    [StringLength(20)]
    public string? SortBy { get; set; }

    /// <summary>"asc" or "desc".</summary>
    [RegularExpression(@"^(asc|desc)?$", ErrorMessage = "SortOrder must be 'asc', 'desc', or empty")]
    public string? SortOrder { get; set; }

    // --- Pagination ---
    [Range(1, 1000)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 25;

    // --- Results ---
    public bool HasSearched { get; set; }
    public string? Notice { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public bool HasValidationErrors => ValidationErrors.Count > 0;
    public string? UsedAirportCode { get; set; }
    public List<AeroDataBoxFlight> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public PaginationState Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalCount = TotalCount
    };

    // --- Display helpers (moved here from Razor view) ---

    public static string GetStatusClass(string rawStatus) => FlightFormattingHelpers.GetStatusClass(rawStatus);

    public static string GetStatusLabel(string rawStatus) => FlightFormattingHelpers.GetStatusLabel(rawStatus);

    public static string ConvertToIata(string? code) => FlightFormattingHelpers.ConvertToIata(code);

    public static DateTime? ParseLocalDate(string? value) => FlightFormattingHelpers.ParseLocalDate(value);
}
