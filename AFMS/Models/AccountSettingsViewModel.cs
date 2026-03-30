namespace AFMS.Models;

public class AccountSettingsViewModel
{
    public string ThemePreference { get; set; } = string.Empty;

    public int SessionExpiryHours { get; set; }

    public string AuthCookieName { get; set; } = string.Empty;

    public string JwtIssuer { get; set; } = string.Empty;

    public string JwtAudience { get; set; } = string.Empty;

    public DateTime LastReviewedUtc { get; set; }
}
