// Core policy entity tracking coverage amounts, premium, status, dates, agent assignment, and auto-renewal preference.
using Domain.Enums;

namespace Domain.Entities
{
    public class Policy
    {
        public string Id { get; set; } = string.Empty;
        // Human-readable policy number auto-generated as POL-XXXXXXXX upon issuance
        public string PolicyNumber { get; set; } = string.Empty;
        // Foreign key to the customer who owns this policy
        public string UserId { get; set; } = string.Empty;
        // Foreign key to the selected coverage plan
        public string PlanId { get; set; } = string.Empty;
        // Foreign key to the business profile used during purchase; drives risk multipliers
        public string? BusinessProfileId { get; set; }
        // Agent assigned to manage this policy; set from the customer's AssignedAgentId during creation
        public string? AgentId { get; set; }
        // Tracks who initiated the policy (typically the customer themselves)
        public string CreatedByUserId { get; set; } = string.Empty;
        // The coverage amount chosen by the customer within the plan's min/max range
        public decimal SelectedCoverageAmount { get; set; }
        // Final annual premium after applying all risk multipliers and business profile adjustments
        public decimal PremiumAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        // Current lifecycle state of the policy (PendingReview → Approved → Active → Expired)
        public PolicyStatus Status { get; set; } = PolicyStatus.PendingReview;
        // Populated by the agent when rejecting a policy with a mandatory explanation
        public string? RejectionReason { get; set; }
        // Agent's commission computed as a percentage of the annual premium
        public decimal CommissionAmount { get; set; }
        // Tracks whether the commission has been paid out to the agent
        public CommissionStatus CommissionStatus { get; set; } = CommissionStatus.Pending;
        // When true the policy will auto-renew before its EndDate
        public bool AutoRenew { get; set; } = false;
        // Payment cadence: Monthly, Quarterly, HalfYearly, or Annually
        public string PaymentFrequency { get; set; } = "Monthly";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Policy Renewal Fields
        // Links to the original policy when this is a renewal
        public string? PreviousPolicyId { get; set; }
        // True if this policy was created as a renewal of an expired or about-to-expire policy
        public bool IsRenewal { get; set; } = false;

        // Navigation properties
        public User User { get; set; } = null!;
        public Plan Plan { get; set; } = null!;
        public BusinessProfile BusinessProfile { get; set; } = null!;
        public User? Agent { get; set; }
        public User CreatedByUser { get; set; } = null!;
        public Policy? PreviousPolicy { get; set; }
        // Collections: all claims filed, payments made, and documents uploaded for this policy
        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
