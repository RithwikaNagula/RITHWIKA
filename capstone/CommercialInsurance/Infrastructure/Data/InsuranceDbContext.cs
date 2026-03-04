// Provides core functionality and structures for the application.
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class InsuranceDbContext : DbContext
    {
        public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<InsuranceType> InsuranceTypes => Set<InsuranceType>();
        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<Claim> Claims => Set<Claim>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<BusinessProfile> BusinessProfiles => Set<BusinessProfile>();
        public DbSet<ClaimHistoryLog> ClaimHistoryLogs => Set<ClaimHistoryLog>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<SupportInquiry> SupportInquiries => Set<SupportInquiry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from the Configurations folder
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InsuranceDbContext).Assembly);
        }
    }
}
