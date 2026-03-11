// A specific coverage offering within an insurance type, defining premium, coverage range, and policy duration.
namespace Domain.Entities
{
    public class Plan
    {
        public string Id { get; set; } = string.Empty;
        // Display name for this plan (e.g., "Standard Coverage", "Premium Plus")
        public string PlanName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        // Minimum and maximum coverage amounts a customer can select under this plan
        public decimal MinCoverageAmount { get; set; }
        public decimal MaxCoverageAmount { get; set; }
        // Starting premium before risk multipliers are applied during quote generation
        public decimal BasePremium { get; set; }
        // Policy validity period; determines the EndDate relative to StartDate
        public int DurationInMonths { get; set; }
        // Foreign key to the parent insurance category
        public string InsuranceTypeId { get; set; } = string.Empty;
        // Soft-delete flag; inactive plans are hidden from customer-facing pages
        public bool IsActive { get; set; } = true;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public InsuranceType InsuranceType { get; set; } = null!;
        public ICollection<Policy> Policies { get; set; } = new List<Policy>();
    }
}
