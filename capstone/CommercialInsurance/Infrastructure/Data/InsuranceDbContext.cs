// EF Core DbContext exposing all entity sets; applies Fluent API configurations from EntityConfigurations and manages database access.
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    // Central EF Core context; registered as a scoped service in DI and injected into all repositories
    public class InsuranceDbContext : DbContext
    {
        public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : base(options) { }

        // Identity and access control
        public DbSet<User> Users => Set<User>();

        // Insurance catalog
        public DbSet<InsuranceType> InsuranceTypes => Set<InsuranceType>();
        public DbSet<Plan> Plans => Set<Plan>();

        // Policy lifecycle
        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<Claim> Claims => Set<Claim>();
        public DbSet<Payment> Payments => Set<Payment>();

        // Supporting entities
        public DbSet<BusinessProfile> BusinessProfiles => Set<BusinessProfile>();
        public DbSet<ClaimHistoryLog> ClaimHistoryLogs => Set<ClaimHistoryLog>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<SupportInquiry> SupportInquiries => Set<SupportInquiry>();

        // Applies all IEntityTypeConfiguration<T> classes from the Infrastructure assembly
        // so relationship rules, indexes, and column types are defined in dedicated configuration classes
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InsuranceDbContext).Assembly);
        }
    }
}
