namespace AFMS.Models;

public class AccountProfileViewModel
{
    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int SessionExpiryHours { get; set; }

    public DateTime LastUpdatedUtc { get; set; }
}
