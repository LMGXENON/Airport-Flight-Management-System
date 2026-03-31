namespace AFMS.Models;

public class AccountSettingsViewModel
{
    public string Theme { get; set; } = "light";

    public string DefaultAirport { get; set; } = "EGLL";

    public string TimeFormat { get; set; } = "24";

    public string Language { get; set; } = "en";

    public bool Saved { get; set; }
}
