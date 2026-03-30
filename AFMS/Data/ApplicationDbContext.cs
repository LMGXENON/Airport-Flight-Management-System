using AFMS.Models;
using Microsoft.EntityFrameworkCore;

namespace AFMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Flight> Flights { get; set; }

        public DbSet<AuthCredential> AuthCredentials { get; set; }

        public DbSet<LoginAuditEntry> LoginAuditEntries { get; set; }
    }
}
