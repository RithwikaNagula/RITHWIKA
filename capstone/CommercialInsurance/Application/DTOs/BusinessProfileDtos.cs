// DTOs for creating, reading, and updating a customer's business profile (company name, type, address, employee count, etc.).
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    // Outbound DTO representing a fully hydrated business profile for display on the customer dashboard
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

    // Inbound DTO for creating a new business profile; all fields are required before a policy can be issued
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

    // Inbound DTO for editing an existing business profile; includes the profile ID for lookup
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
