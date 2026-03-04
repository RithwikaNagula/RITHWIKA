// this class defines the structure of an insurance policy and all its related data points
using Domain.Enums;

namespace Domain.Entities
{
    public class Policy
    {
        public string Id { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        // the identity of the user who owns this insurance policy
        public string UserId { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string? BusinessProfileId { get; set; }
        public string? AgentId { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public decimal SelectedCoverageAmount { get; set; }
        // the total price that the user must pay for this insurance policy coverage
        public decimal PremiumAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        // the current state of the policy such as pending active or expired
        public PolicyStatus Status { get; set; } = PolicyStatus.PendingReview;
        public string? RejectionReason { get; set; }
        public decimal CommissionAmount { get; set; }
        public CommissionStatus CommissionStatus { get; set; } = CommissionStatus.Pending;
        public bool AutoRenew { get; set; } = false;
        public string PaymentFrequency { get; set; } = "Monthly";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Policy Renewal Fields
        public string? PreviousPolicyId { get; set; }
        public bool IsRenewal { get; set; } = false;

        // Navigation properties
        public User User { get; set; } = null!;
        public Plan Plan { get; set; } = null!;
        public BusinessProfile BusinessProfile { get; set; } = null!;
        public User? Agent { get; set; }
        public User CreatedByUser { get; set; } = null!;
        public Policy? PreviousPolicy { get; set; }
        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
