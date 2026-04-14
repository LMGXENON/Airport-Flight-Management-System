namespace AFMS.Models;

public class AccountProfileViewModel
{
    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int SessionExpiryHours { get; set; }

    public DateTime? SessionExpiresUtc { get; set; }

    public int SessionRemainingSeconds { get; set; }

    public DateTime? LastLoginUtc { get; set; }

    public DateTime LastUpdatedUtc { get; set; }

    public List<LoginHistoryItem> LoginHistory { get; set; } = [];

    public bool PasswordChanged { get; set; }

    public string? PasswordChangeError { get; set; }

    public ChangePasswordViewModel ChangePassword { get; set; } = new();
}

public class LoginHistoryItem
{
    public DateTime OccurredUtc { get; set; }

    public string IpAddress { get; set; } = string.Empty;

    public string UserAgent { get; set; } = string.Empty;

    public string DeviceBrowser { get; set; } = "Unknown device";

    public string Location { get; set; } = "Unknown";

    public bool IsCurrentSession { get; set; }
}
