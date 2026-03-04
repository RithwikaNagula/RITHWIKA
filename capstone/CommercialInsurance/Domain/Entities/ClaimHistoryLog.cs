// Provides core functionality and structures for the application.
using Domain.Enums;

namespace Domain.Entities
{
    public class ClaimHistoryLog
    {
        public string Id { get; set; } = string.Empty;
        public string ClaimId { get; set; } = string.Empty;
        public ClaimStatus Status { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public string ChangedByUserId { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Claim Claim { get; set; } = null!;
        public User ChangedByUser { get; set; } = null!;
    }
}
