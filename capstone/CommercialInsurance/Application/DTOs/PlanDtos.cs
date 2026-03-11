// DTOs for insurance plans: attributes such as coverage range, base premium, duration, and description.
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    // Inbound DTO for defining a new plan under an insurance type with coverage limits and pricing
    public class CreatePlanDto
    {
        [Required, StringLength(100)]
        public string PlanName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal MinCoverageAmount { get; set; }

        [Required]
        public decimal MaxCoverageAmount { get; set; }

        [Required]
        public decimal BasePremium { get; set; }

        [Required]
        public int DurationInMonths { get; set; }

        [Required]
        public string InsuranceTypeId { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
    }

    // Outbound DTO representing a plan with its parent insurance type name and active status
    public class PlanDto
    {
        public string Id { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal MinCoverageAmount { get; set; }
        public decimal MaxCoverageAmount { get; set; }
        public decimal BasePremium { get; set; }
        public int DurationInMonths { get; set; }
        public string InsuranceTypeId { get; set; } = string.Empty;
        public string InsuranceTypeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
