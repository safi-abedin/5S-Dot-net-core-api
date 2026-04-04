using api.Models.Audits;
using api.Models.Checklists;
using api.Models.Companies;
using api.Models.Logs;
using api.Models.RedTags;
using api.Models.Users;
using api.Models.Zones;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Zone> Zones { get; set; }

        public DbSet<ChecklistCategory> ChecklistCategories { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }

        public DbSet<Audit> Audits { get; set; }
        public DbSet<AuditItem> AuditItems { get; set; }

        public DbSet<RedTag> RedTags { get; set; }

        public DbSet<AppLog> AppLogs { get; set; }
    }
}
