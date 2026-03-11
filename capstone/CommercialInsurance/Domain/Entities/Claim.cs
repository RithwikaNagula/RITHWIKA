// Represents an insurance claim filed against a policy, including amount, description, status, and document evidence.
using Domain.Enums;

namespace Domain.Entities
{
    public class Claim
    {
        public string Id { get; set; } = string.Empty;
        // Human-readable identifier auto-generated as CLM-XXXXXXXX
        public string ClaimNumber { get; set; } = string.Empty;
        // Foreign key to the policy this claim is filed against
        public string PolicyId { get; set; } = string.Empty;
        // Customer-provided explanation of the incident or loss
        public string Description { get; set; } = string.Empty;
        // Monetary value requested by the customer; must not exceed remaining coverage
        public decimal ClaimAmount { get; set; }
        // Lifecycle state managed by the claims officer (Submitted → UnderReview → Approved/Rejected → Settled)
        public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
        // ID of the claims officer assigned to review this claim; auto-assigned from the customer's record
        public string? ClaimsOfficerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Populated when the claim reaches a terminal status (Approved, Rejected, or Settled)
        public DateTime? ResolvedAt { get; set; }

        // Navigation properties
        public Policy Policy { get; set; } = null!;
        public User? ClaimsOfficer { get; set; }
        public ICollection<ClaimHistoryLog> ClaimHistoryLogs { get; set; } = new List<ClaimHistoryLog>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
