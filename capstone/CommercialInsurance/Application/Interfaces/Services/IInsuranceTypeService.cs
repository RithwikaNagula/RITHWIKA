// Provides core functionality and structures for the application.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IInsuranceTypeService
    {
        Task<InsuranceTypeDto> CreateInsuranceTypeAsync(CreateInsuranceTypeDto dto);
        Task<IEnumerable<InsuranceTypeDto>> GetAllInsuranceTypesAsync();
        Task<InsuranceTypeDto?> GetInsuranceTypeByIdAsync(string id);
        Task<bool> DeleteInsuranceTypeAsync(string id);
    }
}
