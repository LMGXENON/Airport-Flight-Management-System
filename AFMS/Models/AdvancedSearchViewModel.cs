namespace AFMS.Models;

public class AdvancedSearchViewModel
{
    // --- Filter properties ---
    public string? Flight { get; set; }
    public string? Airline { get; set; }
    public string? Destination { get; set; }
    public DateTime? DepartureDate { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public string? Terminal { get; set; }

    public List<string> Statuses { get; set; } = new();

    /// <summary>"Departure", "Arrival", or null for both.</summary>
    public string? Direction { get; set; }

    /// <summary>Earliest scheduled time to include, formatted as "HH:mm".</summary>
    public string? TimeRangeStart { get; set; }

    /// <summary>Latest scheduled time to include, formatted as "HH:mm".</summary>
    public string? TimeRangeEnd { get; set; }

    // --- Sorting ---
    /// <summary>Column to sort by: flight | airline | departure | arrival | status.</summary>
    public string? SortBy { get; set; }

    /// <summary>"asc" or "desc".</summary>
    public string? SortOrder { get; set; }

    // --- Pagination ---
    public int Page { get; set; } = 1;
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
