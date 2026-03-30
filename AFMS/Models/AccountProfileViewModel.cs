namespace AFMS.Models;

public class AccountProfileViewModel
{
    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int SessionExpiryHours { get; set; }

    public DateTime? LastLoginUtc { get; set; }

    public DateTime LastUpdatedUtc { get; set; }
}
