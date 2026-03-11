// Fluent API configuration for all entities: primary keys, required fields, string lengths, relationships (one-to-many, one-to-one), and enum conversions.
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    // Defines the precise database schema limits and relationships for the core User table.
    // Handles one-to-many relationship cascades for Role assignment tracking.
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
            // Enforce unique email constraint at the database level to prevent duplicate registrations
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.PasswordHash).IsRequired();
            // Store enum as a readable string column rather than an integer for easier debugging
            builder.Property(u => u.Role).IsRequired().HasConversion<string>().HasMaxLength(20);

            // Customer → Agent: Restrict delete so removing an agent does not cascade-delete customers
            builder.HasOne(u => u.AssignedAgent)
                   .WithMany(a => a.AssignedCustomers)
                   .HasForeignKey(u => u.AssignedAgentId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Customer → ClaimsOfficer: same restrict behavior to protect customer records
            builder.HasOne(u => u.AssignedClaimsOfficer)
                   .WithMany(o => o.AssignedOfficerCustomers)
                   .HasForeignKey(u => u.AssignedClaimsOfficerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    // Maps the top-level categories of insurance available on the platform and enforces naming uniqueness.
    public class InsuranceTypeConfiguration : IEntityTypeConfiguration<InsuranceType>
    {
        public void Configure(EntityTypeBuilder<InsuranceType> builder)
        {
            builder.HasKey(i => i.Id);
            builder.Property(i => i.TypeName).IsRequired().HasMaxLength(100);
            builder.Property(i => i.Description).HasMaxLength(500);
            // Prevent duplicate insurance type names (e.g., two "General Liability" entries)
            builder.HasIndex(i => i.TypeName).IsUnique();
        }
    }

    // Configures the specific plans nested under Insurance Types, locking in precision for financial caps.
    public class PlanConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.PlanName).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Description).HasMaxLength(500);
            // Use decimal(18,2) for all monetary fields to avoid floating-point rounding errors
            builder.Property(p => p.MinCoverageAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.MaxCoverageAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.BasePremium).HasColumnType("decimal(18,2)");

            // Restrict delete: cannot remove an InsuranceType that still has active plans
            builder.HasOne(p => p.InsuranceType)
                   .WithMany(i => i.Plans)
                   .HasForeignKey(p => p.InsuranceTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    // Extremely dense relationship mapping for the core Policy entity.
    // Restricts cascade deletions heavily to ensure financial and audit compliance records persist natively.
    public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
    {
        public void Configure(EntityTypeBuilder<Policy> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.PolicyNumber).IsRequired().HasMaxLength(50);
            // Unique index on PolicyNumber for fast lookups and to prevent duplicates
            builder.HasIndex(p => p.PolicyNumber).IsUnique();
            // Store enums as readable strings instead of integers
            builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(p => p.SelectedCoverageAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.PremiumAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.CommissionAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.CommissionStatus).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(p => p.RejectionReason).HasMaxLength(500);

            // BusinessProfile is optional (nullable FK) because agents can create policies without one
            builder.HasOne(p => p.BusinessProfile)
                   .WithMany()
                   .HasForeignKey(p => p.BusinessProfileId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired(false);

            // Policy → Customer: Restrict so deleting a user does not orphan financial records
            builder.HasOne(p => p.User)
                   .WithMany(u => u.CustomerPolicies)
                   .HasForeignKey(p => p.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Policy → Agent: Restrict because commission records must persist
            builder.HasOne(p => p.Agent)
                   .WithMany(u => u.AgentPolicies)
                   .HasForeignKey(p => p.AgentId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Policy → Creator: tracks who initiated the policy for audit purposes
            builder.HasOne(p => p.CreatedByUser)
                   .WithMany(u => u.CreatedPolicies)
                   .HasForeignKey(p => p.CreatedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Policy → Plan: Restrict so a plan with existing policies cannot be deleted
            builder.HasOne(p => p.Plan)
                   .WithMany(pl => pl.Policies)
                   .HasForeignKey(p => p.PlanId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing FK for policy renewals; nullable because most policies are not renewals
            builder.HasOne(p => p.PreviousPolicy)
                   .WithMany()
                   .HasForeignKey(p => p.PreviousPolicyId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired(false);
        }
    }

    // Sets up cascading deletes for system notifications so that deleting a user securely purges their UI alerts.
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
            builder.Property(n => n.Message).IsRequired().HasMaxLength(1000);
            builder.Property(n => n.Type).IsRequired().HasConversion<string>().HasMaxLength(20);

            // Cascade: when a user is deleted, all their notifications are automatically removed
            builder.HasOne(n => n.User)
                   .WithMany(u => u.Notifications)
                   .HasForeignKey(n => n.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    // Handles the complex relationship chaining an Insurance Claim to both a Policy constraint and reviewing Officer context.
    public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
    {
        public void Configure(EntityTypeBuilder<Claim> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.ClaimNumber).IsRequired().HasMaxLength(50);
            // Unique index so CLM-XXXXXXXX numbers cannot collide
            builder.HasIndex(c => c.ClaimNumber).IsUnique();
            builder.Property(c => c.Description).IsRequired().HasMaxLength(1000);
            builder.Property(c => c.ClaimAmount).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

            // Cascade: deleting a policy removes all its claims (used by admin cascade-delete)
            builder.HasOne(c => c.Policy)
                   .WithMany(p => p.Claims)
                   .HasForeignKey(c => c.PolicyId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Restrict: claims officer records must remain even if the claim is later deleted
            builder.HasOne(c => c.ClaimsOfficer)
                   .WithMany(u => u.ReviewedClaims)
                   .HasForeignKey(c => c.ClaimsOfficerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    // Secures financial history constraints tying payments tightly beneath their parent Policy.
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(p => p.TransactionId).HasMaxLength(100);

            // Cascade: deleting a policy removes all its payment records
            builder.HasOne(p => p.Policy)
                   .WithMany(pol => pol.Payments)
                   .HasForeignKey(p => p.PolicyId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    // Configures the strict append-only audit trail logs for State transitions on complex domains.
    public class ClaimHistoryLogConfiguration : IEntityTypeConfiguration<ClaimHistoryLog>
    {
        public void Configure(EntityTypeBuilder<ClaimHistoryLog> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(c => c.Remarks).HasMaxLength(1000);

            // Restrict: audit logs must not be deleted when a claim is removed
            builder.HasOne(c => c.Claim)
                   .WithMany(cl => cl.ClaimHistoryLogs)
                   .HasForeignKey(c => c.ClaimId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Restrict: keep the reference to who made each status change
            builder.HasOne(c => c.ChangedByUser)
                   .WithMany(u => u.ChangedClaimLogs)
                   .HasForeignKey(c => c.ChangedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    // Virtual file-storage map configuration tracking Document URIs for disk-based attachments.
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(d => d.FileName).IsRequired().HasMaxLength(255);
            builder.Property(d => d.FilePath).IsRequired().HasMaxLength(500);
            builder.Property(d => d.FileType).HasMaxLength(50);

            // ClientCascade: EF handles the cascade in memory so the DB doesn't need a trigger
            // Both FKs are optional because a document can belong to a policy, a claim, or both
            builder.HasOne(d => d.Policy)
                   .WithMany(p => p.Documents)
                   .HasForeignKey(d => d.PolicyId)
                   .OnDelete(DeleteBehavior.ClientCascade)
                   .IsRequired(false);

            builder.HasOne(d => d.Claim)
                   .WithMany(c => c.Documents)
                   .HasForeignKey(d => d.ClaimId)
                   .OnDelete(DeleteBehavior.ClientCascade)
                   .IsRequired(false);
        }
    }
}
