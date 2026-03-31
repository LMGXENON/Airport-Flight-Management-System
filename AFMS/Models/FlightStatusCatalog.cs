namespace AFMS.Models;

public static class FlightStatusCatalog
{
    public sealed record StatusOption(string Value, string Label, string CssClass);

    private static readonly IReadOnlyList<StatusOption> AllStatuses =
    [
        new("Scheduled",  "Scheduled",  "status-scheduled"),
        new("Boarding",   "Boarding",   "status-boarding"),
        new("Departed",   "Departed",   "status-departed"),
        new("InFlight",   "In Flight",  "status-inflight"),
        new("Approaching","Approaching","status-approaching"),
        new("Arrived",    "Arrived",    "status-arrived"),
        new("Delayed",    "Delayed",    "status-delayed"),
        new("Canceled",   "Canceled",   "status-cancelled")
    ];

    private static readonly IReadOnlyDictionary<string, string> AliasToCanonical =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["scheduled"] = "Scheduled",
            ["expected"] = "Scheduled",
            ["ontime"] = "Scheduled",
            ["on time"] = "Scheduled",
            ["on-time"] = "Scheduled",
            ["onschedule"] = "Scheduled",
            ["on schedule"] = "Scheduled",
            ["checkin"] = "Scheduled",
            ["check in"] = "Scheduled",
            ["check-in"] = "Scheduled",

            ["boarding"] = "Boarding",
            ["finalcall"] = "Boarding",
            ["lastcall"] = "Boarding",
            ["boardingnow"] = "Boarding",

            ["departed"] = "Departed",
            ["departing"] = "Departed",
            ["gateclosed"] = "Departed",
            ["gate closed"] = "Departed",
            ["gateclose"] = "Departed",
            ["gateclosing"] = "Departed",
            ["taxiing"] = "Departed",

            ["airborne"] = "InFlight",
            ["inflight"] = "InFlight",
            ["in flight"] = "InFlight",
            ["enroute"] = "InFlight",
            ["en route"] = "InFlight",
            ["en-route"] = "InFlight",

            ["approaching"] = "Approaching",

            ["arrived"] = "Arrived",
            ["arriving"] = "Arrived",
            ["landed"] = "Arrived",
            ["landing"] = "Arrived",
            ["atgate"] = "Arrived",

            ["delayed"] = "Delayed",
            ["late"] = "Delayed",
            ["runninglate"] = "Delayed",

            ["canceled"] = "Canceled",
            ["cancelled"] = "Canceled",
            ["cancellation"] = "Canceled",
            ["cancelation"] = "Canceled",
            ["canceluncertain"] = "Canceled",
            ["canceleduncertain"] = "Canceled",
            ["cancelleduncertain"] = "Canceled",
            ["diverted"] = "Canceled"
        };

    private static readonly IReadOnlyDictionary<string, string> CanonicalByNormalizedKey =
        AllStatuses.ToDictionary(
            status => NormalizeKey(status.Value),
            status => status.Value,
            StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlySet<string> CanonicalNormalizedKeys =
        new HashSet<string>(CanonicalByNormalizedKey.Keys, StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyDictionary<string, StatusOption> OptionByValue =
        AllStatuses.ToDictionary(status => status.Value, status => status, StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyList<string> AllStatusValues =
        AllStatuses.Select(status => status.Value).ToList();

    public static IReadOnlyList<StatusOption> Options => AllStatuses;

    public static IReadOnlyList<string> Values => AllStatusValues;

    public static string Normalize(string? value)
    {
        var key = NormalizeKey(value);
        if (string.IsNullOrWhiteSpace(key))
            return "Scheduled";

        if (AliasToCanonical.TryGetValue(key, out var canonical))
            return canonical;

        return CanonicalByNormalizedKey.TryGetValue(key, out var directMatch)
            ? directMatch
            : "Scheduled";
    }

    public static List<string> NormalizeStatuses(IEnumerable<string>? statuses) =>
        (statuses ?? Array.Empty<string>())
            .Where(status => !string.IsNullOrWhiteSpace(status))
            .Where(IsKnown)
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
            || CanonicalNormalizedKeys.Contains(key);
    }

    private static StatusOption GetOption(string? value)
    {
        var normalized = Normalize(value);
        return OptionByValue.TryGetValue(normalized, out var option)
            ? option
            : AllStatuses[0];
    }

    private static string NormalizeKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    }
}