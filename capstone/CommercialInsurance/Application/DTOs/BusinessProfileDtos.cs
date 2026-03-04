// Provides core functionality and structures for the application.
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class BusinessProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public decimal AnnualRevenue { get; set; }
        public int EmployeeCount { get; set; }
        public string City { get; set; } = string.Empty;
        public bool IsProfileCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateBusinessProfileDto
    {
        [Required]
        [MaxLength(100)]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Industry { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal AnnualRevenue { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int EmployeeCount { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
    }

    public class UpdateBusinessProfileDto
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Industry { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal AnnualRevenue { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int EmployeeCount { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        public bool IsProfileCompleted { get; set; }
    }
}
