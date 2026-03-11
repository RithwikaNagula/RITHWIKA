// Optional customer-owned profile capturing business details required before requesting a policy quote.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class BusinessProfile
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        // Foreign key linking this profile to the owning customer
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        // Legal or trade name of the business entity
        [Required]
        [MaxLength(100)]
        public string BusinessName { get; set; } = string.Empty;

        // Industry category (e.g., Manufacturing, IT, Healthcare) used in premium risk calculations
        [Required]
        [MaxLength(50)]
        public string Industry { get; set; } = string.Empty;

        // Yearly revenue; affects the revenue risk multiplier during premium calculation
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AnnualRevenue { get; set; }

        // Total headcount; affects the employee count risk multiplier during premium calculation
        [Required]
        public int EmployeeCount { get; set; }

        // City where the business operates; stored for reference but not currently used in risk scoring
        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        // Set to true once all required fields are filled; a true value is required before creating a policy
        public bool IsProfileCompleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
