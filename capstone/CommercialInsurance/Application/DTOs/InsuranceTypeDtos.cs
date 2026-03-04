using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreateInsuranceTypeDto
    {
        [Required, StringLength(100)]
        public string TypeName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
    }

    public class InsuranceTypeDto
    {
        public string Id { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
