using System.ComponentModel.DataAnnotations;

namespace AFMS.Models;

public class LoginAuditEntry
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [StringLength(120)]
    public string IpAddress { get; set; } = string.Empty;

    [StringLength(512)]
    public string UserAgent { get; set; } = string.Empty;

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}
