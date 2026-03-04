// Provides core functionality and structures for the application.
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.Role).IsRequired().HasConversion<string>().HasMaxLength(20);

            builder.HasOne(u => u.AssignedAgent)
                   .WithMany(a => a.AssignedCustomers)
                   .HasForeignKey(u => u.AssignedAgentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.AssignedClaimsOfficer)
                   .WithMany(o => o.AssignedOfficerCustomers)
                   .HasForeignKey(u => u.AssignedClaimsOfficerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class InsuranceTypeConfiguration : IEntityTypeConfiguration<InsuranceType>
    {
        public void Configure(EntityTypeBuilder<InsuranceType> builder)
        {
            builder.HasKey(i => i.Id);
            builder.Property(i => i.TypeName).IsRequired().HasMaxLength(100);
            builder.Property(i => i.Description).HasMaxLength(500);
            builder.HasIndex(i => i.TypeName).IsUnique();
        }
    }

    public class PlanConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.PlanName).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Description).HasMaxLength(500);
            builder.Property(p => p.MinCoverageAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.MaxCoverageAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.BasePremium).HasColumnType("decimal(18,2)");

            builder.HasOne(p => p.InsuranceType)
                   .WithMany(i => i.Plans)
                   .HasForeignKey(p => p.InsuranceTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
    {
        public void Configure(EntityTypeBuilder<Policy> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.PolicyNumber).IsRequired().HasMaxLength(50);
            builder.HasIndex(p => p.PolicyNumber).IsUnique();
            builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(p => p.SelectedCoverageAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.PremiumAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.CommissionAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.CommissionStatus).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(p => p.RejectionReason).HasMaxLength(500);

            builder.HasOne(p => p.BusinessProfile)
                   .WithMany()
                   .HasForeignKey(p => p.BusinessProfileId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired(false);

            builder.HasOne(p => p.User)
                   .WithMany(u => u.CustomerPolicies)
                   .HasForeignKey(p => p.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Agent)
                   .WithMany(u => u.AgentPolicies)
                   .HasForeignKey(p => p.AgentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.CreatedByUser)
                   .WithMany(u => u.CreatedPolicies)
                   .HasForeignKey(p => p.CreatedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Plan)
                   .WithMany(pl => pl.Policies)
                   .HasForeignKey(p => p.PlanId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.PreviousPolicy)
                   .WithMany()
                   .HasForeignKey(p => p.PreviousPolicyId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired(false);
        }
    }

    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
            builder.Property(n => n.Message).IsRequired().HasMaxLength(1000);
            builder.Property(n => n.Type).IsRequired().HasConversion<string>().HasMaxLength(20);

            builder.HasOne(n => n.User)
                   .WithMany(u => u.Notifications)
                   .HasForeignKey(n => n.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
    {
        public void Configure(EntityTypeBuilder<Claim> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.ClaimNumber).IsRequired().HasMaxLength(50);
            builder.HasIndex(c => c.ClaimNumber).IsUnique();
            builder.Property(c => c.Description).IsRequired().HasMaxLength(1000);
            builder.Property(c => c.ClaimAmount).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

            builder.HasOne(c => c.Policy)
                   .WithMany(p => p.Claims)
                   .HasForeignKey(c => c.PolicyId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.ClaimsOfficer)
                   .WithMany(u => u.ReviewedClaims)
                   .HasForeignKey(c => c.ClaimsOfficerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(p => p.TransactionId).HasMaxLength(100);

            builder.HasOne(p => p.Policy)
                   .WithMany(pol => pol.Payments)
                   .HasForeignKey(p => p.PolicyId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ClaimHistoryLogConfiguration : IEntityTypeConfiguration<ClaimHistoryLog>
    {
        public void Configure(EntityTypeBuilder<ClaimHistoryLog> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(c => c.Remarks).HasMaxLength(1000);

            builder.HasOne(c => c.Claim)
                   .WithMany(cl => cl.ClaimHistoryLogs)
                   .HasForeignKey(c => c.ClaimId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.ChangedByUser)
                   .WithMany(u => u.ChangedClaimLogs)
                   .HasForeignKey(c => c.ChangedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(d => d.FileName).IsRequired().HasMaxLength(255);
            builder.Property(d => d.FilePath).IsRequired().HasMaxLength(500);
            builder.Property(d => d.FileType).HasMaxLength(50);

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
