// this class represents an insurance claim and stores all its relevant information in the database
using Domain.Enums;

namespace Domain.Entities
{
    public class Claim
    {
        // the unique identifier used to track this claim in the system
        public string Id { get; set; } = string.Empty;
        public string ClaimNumber { get; set; } = string.Empty;
        // the identity of the policy that this claim is being made against
        public string PolicyId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        // the total amount of money being requested in this insurance claim
        public decimal ClaimAmount { get; set; }
        public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
        public string? ClaimsOfficerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        // Navigation properties
        public Policy Policy { get; set; } = null!;
        public User? ClaimsOfficer { get; set; }
        public ICollection<ClaimHistoryLog> ClaimHistoryLogs { get; set; } = new List<ClaimHistoryLog>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
