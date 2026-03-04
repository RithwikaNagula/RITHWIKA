// Provides core functionality and structures for the application.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class BusinessProfile
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [MaxLength(100)]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Industry { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AnnualRevenue { get; set; }

        [Required]
        public int EmployeeCount { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        public bool IsProfileCompleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
