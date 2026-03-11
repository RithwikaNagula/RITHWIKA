// DTOs covering claim submission, detail view, status updates, and document attachment for the claims workflow.
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    // Inbound DTO for filing a new claim; description and amount are validated before submission
    public class CreateClaimDto
    {
        [Required, StringLength(2000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Required, Range(1, 10000000)]
        public decimal ClaimAmount { get; set; }
    }

    // Outbound DTO with full claim details including status, officer info, audit history, and attached documents
    public class ClaimDto
    {
        public string Id { get; set; } = string.Empty;
        public string ClaimNumber { get; set; } = string.Empty;
        public string PolicyId { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal ClaimAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ClaimsOfficerId { get; set; }
        public string? ClaimsOfficerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public decimal AnnualRevenue { get; set; }
        public List<ClaimHistoryLogDto> History { get; set; } = new List<ClaimHistoryLogDto>();
        public List<DocumentDto> Documents { get; set; } = new List<DocumentDto>();
    }

    // Inbound DTO for transitioning a claim's status (e.g., UnderReview, Approved, Rejected, Settled)
    public class UpdateClaimStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }

    // Outbound DTO representing one audit-trail entry for a claim status change
    public class ClaimHistoryLogDto
    {
        public string Id { get; set; } = string.Empty;
        public string ClaimId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public string ChangedByUserId { get; set; } = string.Empty;
        public string ChangedByUserName { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }
}
