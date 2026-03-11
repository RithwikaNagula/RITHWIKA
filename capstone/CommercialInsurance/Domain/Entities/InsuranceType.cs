// Top-level insurance category (e.g. Auto, General Liability) that groups related plans.
namespace Domain.Entities
{
    public class InsuranceType
    {
        public string Id { get; set; } = string.Empty;
        // Display name shown in dropdowns and admin panels (e.g., "General Liability")
        public string TypeName { get; set; } = string.Empty;
        // Brief explanation of what this insurance category covers
        public string Description { get; set; } = string.Empty;
        // When false the type and its plans are hidden from customers during quote generation
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation: all plans that belong to this insurance category
        public ICollection<Plan> Plans { get; set; } = new List<Plan>();
    }
}
