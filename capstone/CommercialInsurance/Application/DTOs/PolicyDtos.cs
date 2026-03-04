using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreatePolicyDto
    {
        public string? QuoteId { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        public string PlanId { get; set; } = string.Empty;

        [Required]
        public string BusinessProfileId { get; set; } = string.Empty;

        [Required]
        public decimal SelectedCoverageAmount { get; set; }

        [Required]
        public decimal PremiumAmount { get; set; }

        public string PaymentFrequency { get; set; } = "Monthly";

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool AutoRenew { get; set; }
    }

    public class PremiumCalculationRequestDto
    {
        public string PlanId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string? BusinessProfileId { get; set; }
        [Required]
        public decimal SelectedCoverageAmount { get; set; }
        public string PaymentFrequency { get; set; } = "Monthly"; // "Monthly" or "Yearly"
    }

    public class PremiumCalculationDto
    {
        public string QuoteId { get; set; } = string.Empty;
        public string QuoteNumber { get; set; } = string.Empty;
        public decimal BasePremium { get; set; }
        public decimal CoveragePremium { get; set; }
        public decimal IndustryMultiplier { get; set; }
        public decimal EmployeeCountMultiplier { get; set; }
        public decimal RevenueMultiplier { get; set; }
        public decimal AgentCommissionPercentage { get; set; }
        public decimal AgentCommissionAmount { get; set; }
        public decimal FinalPremium { get; set; }
        public decimal MonthlyPremium { get; set; }
        public decimal YearlyPremium { get; set; }
        public string PaymentFrequency { get; set; } = "Monthly";
    }

    public class PolicyDto
    {
        public string Id { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string? AgentId { get; set; }
        public string? AgentName { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public decimal SelectedCoverageAmount { get; set; }
        public decimal RemainingCoverageAmount { get; set; }
        public decimal PremiumAmount { get; set; }
        public string PaymentFrequency { get; set; } = "Monthly";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public string? BusinessProfileId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public decimal AnnualRevenue { get; set; }
        public decimal CommissionAmount { get; set; }
        public string CommissionStatus { get; set; } = string.Empty;
        public bool AutoRenew { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<DocumentDto> Documents { get; set; } = new List<DocumentDto>();
    }

    public class DocumentDto
    {
        public string Id { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    public class RejectPolicyDto
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }
}
