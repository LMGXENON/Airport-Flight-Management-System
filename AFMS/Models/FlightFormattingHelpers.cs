using System.Globalization;

namespace AFMS.Models;

public static class FlightFormattingHelpers
{
    public static string GetStatusClass(string? rawStatus) => FlightStatusCatalog.GetCssClass(rawStatus);

    public static string GetStatusLabel(string? rawStatus) => FlightStatusCatalog.GetLabel(rawStatus);

    public static string ConvertToIata(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return string.Empty;

        var normalizedCode = new string(code
            .Where(char.IsLetterOrDigit)
            .ToArray())
            .ToUpperInvariant();

        return normalizedCode switch
        {
            "EGLL" => "LHR",
            "KJFK" => "JFK",
            "KLAX" => "LAX",
            "OMDB" => "DXB",
            "EDDF" => "FRA",
            "RJTT" => "HND",
            _ => normalizedCode
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
        if (!parsed.HasValue)
            return fallback;
        if (string.IsNullOrWhiteSpace(format))
            return fallback;
        if (HasUnsupportedFormatTokens(format))
            return fallback;

        try
        {
            return parsed.Value.ToString(format, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return fallback;
        }
    }

    public static string FormatDateTime(DateTime? value, string format, string fallback = "-")
    {
        if (!value.HasValue)
            return fallback;
        if (string.IsNullOrWhiteSpace(format))
            return fallback;
        if (HasUnsupportedFormatTokens(format))
            return fallback;

        try
        {
            return value.Value.ToString(format, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return fallback;
        }
    }

    public static string FormatLocalTime(string? value, string fallback = "-") =>
        FormatLocalDateTime(value, "HH:mm", fallback);

    public static string FormatLocalDateHeader(string? value, string fallback = "-") =>
        FormatLocalDateTime(value, "dddd, MMM dd", fallback);

    private static bool HasUnsupportedFormatTokens(string format) =>
        format.Contains('[') || format.Contains(']');
}