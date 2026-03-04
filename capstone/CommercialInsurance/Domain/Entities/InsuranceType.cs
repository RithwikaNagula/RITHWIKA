// Provides core functionality and structures for the application.
namespace Domain.Entities
{
    public class InsuranceType
    {
        public string Id { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Plan> Plans { get; set; } = new List<Plan>();
    }
}
