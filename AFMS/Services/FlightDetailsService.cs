using AFMS.Models;
using System.Globalization;

namespace AFMS.Services;

/// <summary>
/// Service for formatting and calculating flight details for display.
/// Centralizes all flight detail presentation logic.
/// </summary>
public class FlightDetailsService
{
    private readonly ILogger<FlightDetailsService> _logger;

    public FlightDetailsService(ILogger<FlightDetailsService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calculates flight duration in hours and minutes.
    /// </summary>
    public (int Hours, int Minutes) GetFlightDuration(DateTime departureTime, DateTime arrivalTime)
    {
        var duration = arrivalTime - departureTime;
        return ((int)duration.TotalHours, duration.Minutes);
    }

    /// <summary>
    /// Formats flight duration as a human-readable string.
    /// </summary>
    public string FormatFlightDuration(DateTime departureTime, DateTime arrivalTime)
    {
        var (hours, minutes) = GetFlightDuration(departureTime, arrivalTime);
        return $"{hours}h {minutes}m";
    }

    /// <summary>
    /// Gets display value for nullable fields with fallback.
    /// </summary>
    public string GetDisplayValue(string? value, string fallback = "TBD")
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    /// <summary>
    /// Formats terminal display value.
    /// </summary>
    public string FormatTerminal(string? terminal)
    {
        return $"Terminal {GetDisplayValue(terminal, "1")}";
    }

    /// <summary>
    /// Formats gate display value.
    /// </summary>
    public string FormatGate(string? gate)
    {
        return GetDisplayValue(gate, "TBD");
    }

    /// <summary>
    /// Gets CSS class for flight status.
    /// </summary>
    public string GetStatusClass(string? status)
    {
        return FlightStatusCatalog.GetCssClass(status);
    }

    /// <summary>
    /// Gets human-readable label for flight status.
    /// </summary>
    public string GetStatusLabel(string? status)
    {
        return FlightStatusCatalog.GetLabel(status);
    }

    /// <summary>
    /// Formats date/time for display using specified format.
    /// </summary>
    public string FormatDateTime(DateTime? dateTime, string format, string fallback = "-")
    {
        if (!dateTime.HasValue || !IsSupportedFormat(format))
            return fallback;

        try
        {
            return dateTime.Value.ToString(format, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return fallback;
        }
    }

    private static bool IsSupportedFormat(string format) =>
        !string.IsNullOrWhiteSpace(format)
        && !format.Contains('[')
        && !format.Contains(']');

    /// <summary>
    /// Formats date/time for header display (e.g., "Monday 24 Mar").
    /// </summary>
    public string FormatDateHeader(DateTime dateTime)
    {
        return FormatDateTime(dateTime, "ddd dd MMM");
    }

    /// <summary>
    /// Formats date/time with day and time (e.g., "Mon 24 Mar, 14:30").
    /// </summary>
    public string FormatDateAndTime(DateTime dateTime)
    {
        return FormatDateTime(dateTime, "ddd dd MMM, HH:mm");
    }

    /// <summary>
    /// Validates flight details for integrity.
    /// </summary>
    public FlightDetailsValidation ValidateFlightDetails(Flight flight)
    {
        var validation = new FlightDetailsValidation();

        if (flight == null)
        {
            validation.AddError("Flight data is null");
            return validation;
        }

        if (string.IsNullOrWhiteSpace(flight.FlightNumber))
        {
            validation.AddError("Flight number is missing");
        }

        if (string.IsNullOrWhiteSpace(flight.Airline))
        {
            validation.AddError("Airline is missing");
        }

        if (string.IsNullOrWhiteSpace(flight.Destination))
        {
            validation.AddError("Destination is missing");
        }

        if (!string.IsNullOrWhiteSpace(flight.Origin) && flight.Origin.Length > 100)
        {
            validation.AddWarning("Origin exceeds maximum length");
        }

        if (flight.ArrivalTime <= flight.DepartureTime)
        {
            validation.AddError("Arrival time must be after departure time");
        }

        if ((flight.ArrivalTime - flight.DepartureTime).TotalHours > 24)
        {
            validation.AddWarning("Flight duration exceeds 24 hours");
        }

        return validation;
    }
}

/// <summary>
/// Result of flight details validation.
/// </summary>
public class FlightDetailsValidation
{
    private readonly List<string> _errors = new();
    private readonly List<string> _warnings = new();

    public IReadOnlyList<string> Errors => _errors.AsReadOnly();
    public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();
    public bool IsValid => _errors.Count == 0;

    public void AddError(string message)
    {
        _errors.Add(message);
    }

    public void AddWarning(string message)
    {
        _warnings.Add(message);
    }
}
