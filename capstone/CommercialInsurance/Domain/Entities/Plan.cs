// Provides core functionality and structures for the application.
namespace Domain.Entities
{
    public class Plan
    {
        public string Id { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal MinCoverageAmount { get; set; }
        public decimal MaxCoverageAmount { get; set; }
        public decimal BasePremium { get; set; }
        public int DurationInMonths { get; set; }
        public string InsuranceTypeId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public InsuranceType InsuranceType { get; set; } = null!;
        public ICollection<Policy> Policies { get; set; } = new List<Policy>();
    }
}
