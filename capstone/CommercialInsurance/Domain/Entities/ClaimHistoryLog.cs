// Audit trail entry recording each status change on a claim with timestamp and optional officer notes.
using Domain.Enums;

namespace Domain.Entities
{
    public class ClaimHistoryLog
    {
        public string Id { get; set; } = string.Empty;
        // Foreign key linking this log entry to a specific claim
        public string ClaimId { get; set; } = string.Empty;
        // The status the claim was moved to in this transition
        public ClaimStatus Status { get; set; }
        // Optional notes provided by the officer explaining the status change
        public string Remarks { get; set; } = string.Empty;
        // ID of the user (typically ClaimsOfficer) who made this change
        public string ChangedByUserId { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Claim Claim { get; set; } = null!;
        public User ChangedByUser { get; set; } = null!;
    }
}
