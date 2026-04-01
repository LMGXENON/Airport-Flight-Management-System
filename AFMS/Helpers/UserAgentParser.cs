namespace AFMS.Helpers;

public static class UserAgentParser
{
    public static string ToDeviceBrowserLabel(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return "Unknown browser on Unknown OS";

        var ua = userAgent.Trim();
        var browser = ParseBrowser(ua);
        var os = ParseOperatingSystem(ua);
        return $"{browser} on {os}";
    }

    private static string ParseBrowser(string ua)
    {
        if (ua.Contains("Edg/", StringComparison.OrdinalIgnoreCase))
            return "Edge";

        if (ua.Contains("OPR/", StringComparison.OrdinalIgnoreCase)
            || ua.Contains("Opera", StringComparison.OrdinalIgnoreCase))
            return "Opera";

        if (ua.Contains("Firefox/", StringComparison.OrdinalIgnoreCase))
            return "Firefox";

        if (ua.Contains("CriOS/", StringComparison.OrdinalIgnoreCase)
            || (ua.Contains("Chrome/", StringComparison.OrdinalIgnoreCase)
                && !ua.Contains("Edg/", StringComparison.OrdinalIgnoreCase)
                && !ua.Contains("OPR/", StringComparison.OrdinalIgnoreCase)))
            return "Chrome";

        if ((ua.Contains("Safari/", StringComparison.OrdinalIgnoreCase)
                || ua.Contains("Version/", StringComparison.OrdinalIgnoreCase))
            && !ua.Contains("Chrome/", StringComparison.OrdinalIgnoreCase)
            && !ua.Contains("CriOS/", StringComparison.OrdinalIgnoreCase)
            && !ua.Contains("Edg/", StringComparison.OrdinalIgnoreCase)
            && !ua.Contains("OPR/", StringComparison.OrdinalIgnoreCase))
            return "Safari";

        return "Unknown browser";
    }

    private static string ParseOperatingSystem(string ua)
    {
        if (ua.Contains("Windows", StringComparison.OrdinalIgnoreCase))
            return "Windows";

        if (ua.Contains("Android", StringComparison.OrdinalIgnoreCase))
            return "Android";

        if (ua.Contains("iPhone", StringComparison.OrdinalIgnoreCase)
            || ua.Contains("iPad", StringComparison.OrdinalIgnoreCase)
            || ua.Contains("iOS", StringComparison.OrdinalIgnoreCase))
            return "iOS";

        if (ua.Contains("Mac OS X", StringComparison.OrdinalIgnoreCase)
            || ua.Contains("Macintosh", StringComparison.OrdinalIgnoreCase))
            return "macOS";

        if (ua.Contains("Linux", StringComparison.OrdinalIgnoreCase))
            return "Linux";

        return "Unknown OS";
    }
}
