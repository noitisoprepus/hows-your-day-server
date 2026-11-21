using HowsYourDay.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HowsYourDay.Server.Data
{
    public class HowsYourDayAppDbContext: IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public HowsYourDayAppDbContext(DbContextOptions<HowsYourDayAppDbContext> options) : base(options)
        {
        }

        public DbSet<DayEntry> DayEntries { get; set; }
        public DbSet<DayEntryAnalysis> DayEntryAnalyses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(HowsYourDayAppDbContext).Assembly);

            // Seed IDs are fixed so migrations stay deterministic
            List<IdentityRole<Guid>> roles = new List<IdentityRole<Guid>>
            {
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("9c1b1a2d-6f3e-4b8a-9f0e-2d7c5a4b3e21"),
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("4e2a7c9d-1b3f-4a6e-8c5d-0f9e2b7a1c34"),
                    Name = "User",
                    NormalizedName = "USER"
                }
            };
            builder.Entity<IdentityRole<Guid>>().HasData(roles);
        }
    }
}