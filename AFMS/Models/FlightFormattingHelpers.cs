using System.Globalization;

namespace AFMS.Models;

public static class FlightFormattingHelpers
{
    public static string GetStatusClass(string? rawStatus) => FlightStatusCatalog.GetCssClass(rawStatus);

    public static string GetStatusLabel(string? rawStatus) => FlightStatusCatalog.GetLabel(rawStatus);

    public static string ConvertToIata(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return string.Empty;

        return code.ToUpperInvariant() switch
        {
            "EGLL" => "LHR",
            "KJFK" => "JFK",
            "KLAX" => "LAX",
            "OMDB" => "DXB",
            "EDDF" => "FRA",
            "RJTT" => "HND",
            _ => code
        };
    }

    public static DateTime? ParseLocalDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt)
            ? dt
            : null;
    }

    public static string FormatLocalDateTime(string? value, string format, string fallback = "-")
    {
        var parsed = ParseLocalDate(value);
        return parsed.HasValue ? parsed.Value.ToString(format) : fallback;
    }

    public static string FormatDateTime(DateTime? value, string format, string fallback = "-")
    {
        return value.HasValue ? value.Value.ToString(format) : fallback;
    }

    public static string FormatLocalTime(string? value, string fallback = "-") =>
        FormatLocalDateTime(value, "HH:mm", fallback);

    public static string FormatLocalDateHeader(string? value, string fallback = "-") =>
        FormatLocalDateTime(value, "dddd, MMM dd", fallback);
}