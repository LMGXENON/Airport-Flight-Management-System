namespace AFMS.Models;

public static class FlightStatusCatalog
{
    public sealed record StatusOption(string Value, string Label, string CssClass);

    private static readonly IReadOnlyList<StatusOption> AllStatuses =
    [
        new("Scheduled", "Scheduled", "status-scheduled"),
        new("Boarding", "Boarding", "status-boarding"),
        new("Departed", "Departed", "status-departed"),
        new("Arrived", "Arrived", "status-arrived"),
        new("Delayed", "Delayed", "status-delayed"),
        new("Canceled", "Canceled", "status-cancelled")
    ];

    private static readonly IReadOnlyDictionary<string, string> AliasToCanonical =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["scheduled"] = "Scheduled",
            ["expected"] = "Scheduled",
            ["ontime"] = "Scheduled",
            ["on time"] = "Scheduled",
            ["on-time"] = "Scheduled",
            ["checkin"] = "Scheduled",
            ["check in"] = "Scheduled",
            ["check-in"] = "Scheduled",

            ["boarding"] = "Boarding",

            ["departed"] = "Departed",
            ["enroute"] = "Departed",
            ["en route"] = "Departed",
            ["gateclosed"] = "Departed",
            ["gate closed"] = "Departed",
            ["approaching"] = "Departed",

            ["arrived"] = "Arrived",
            ["landed"] = "Arrived",

            ["delayed"] = "Delayed",

            ["canceled"] = "Canceled",
            ["cancelled"] = "Canceled",
            ["canceleduncertain"] = "Canceled",
            ["diverted"] = "Canceled"
        };

    public static IReadOnlyList<StatusOption> Options => AllStatuses;

    public static IReadOnlyList<string> Values => AllStatuses.Select(status => status.Value).ToList();

    public static string Normalize(string? value)
    {
        var key = NormalizeKey(value);
        if (string.IsNullOrWhiteSpace(key))
            return "Scheduled";

        if (AliasToCanonical.TryGetValue(key, out var canonical))
            return canonical;

        return AllStatuses.Any(status => status.Value.Equals(key, StringComparison.OrdinalIgnoreCase))
            ? AllStatuses.First(status => status.Value.Equals(key, StringComparison.OrdinalIgnoreCase)).Value
            : "Scheduled";
    }

    public static List<string> NormalizeStatuses(IEnumerable<string>? statuses) =>
        (statuses ?? Array.Empty<string>())
            .Select(Normalize)
            .Where(status => !string.IsNullOrWhiteSpace(status))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    public static string GetLabel(string? value) => GetOption(value).Label;

    public static string GetCssClass(string? value) => GetOption(value).CssClass;

    public static bool IsKnown(string? value)
    {
        var key = NormalizeKey(value);
        if (string.IsNullOrWhiteSpace(key))
            return false;

        return AliasToCanonical.ContainsKey(key)
            || AllStatuses.Any(status => NormalizeKey(status.Value).Equals(key, StringComparison.OrdinalIgnoreCase));
    }

    private static StatusOption GetOption(string? value)
    {
        var normalized = Normalize(value);
        return AllStatuses.FirstOrDefault(status => status.Value.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            ?? AllStatuses[0];
    }

    private static string NormalizeKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    }
}