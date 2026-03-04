// Provides core functionality and structures for the application.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IPlanService
    {
        Task<PlanDto> CreatePlanAsync(CreatePlanDto dto);
        Task<IEnumerable<PlanDto>> GetAllPlansAsync();
        Task<IEnumerable<PlanDto>> GetPlansByInsuranceTypeAsync(string insuranceTypeId);
        Task<PlanDto?> GetPlanByIdAsync(string id);
        Task<bool> DeletePlanAsync(string id);
    }
}
