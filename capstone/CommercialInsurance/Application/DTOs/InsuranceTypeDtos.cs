// DTOs for insurance type catalog management: create/update requests and the hydrated type response including nested plans.
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    // Inbound DTO for creating a new insurance category (e.g., General Liability, Property, Workers Comp)
    public class CreateInsuranceTypeDto
    {
        [Required, StringLength(100)]
        public string TypeName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
    }

    // Outbound DTO representing an insurance type with its active/inactive status
    public class InsuranceTypeDto
    {
        public string Id { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
