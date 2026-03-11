// Service contract for managing insurance categories and their plan associations.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IInsuranceTypeService
    {
        // Creates a new insurance category (e.g., General Liability, Workers Comp)
        Task<InsuranceTypeDto> CreateInsuranceTypeAsync(CreateInsuranceTypeDto dto);
        // Returns all insurance types for the plan selection dropdown
        Task<IEnumerable<InsuranceTypeDto>> GetAllInsuranceTypesAsync();
        // Returns a single insurance type by ID
        Task<InsuranceTypeDto?> GetInsuranceTypeByIdAsync(string id);
        // Updates an existing insurance type
        Task<InsuranceTypeDto?> UpdateInsuranceTypeAsync(string id, CreateInsuranceTypeDto dto);
        // Soft or hard deletes an insurance type
        Task<bool> DeleteInsuranceTypeAsync(string id);
    }
}
