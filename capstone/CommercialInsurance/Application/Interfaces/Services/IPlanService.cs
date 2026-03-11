// Service contract for fetching plan details by type, used during quote generation.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IPlanService
    {
        // Creates a new plan under an insurance type with coverage limits and base premium pricing
        Task<PlanDto> CreatePlanAsync(CreatePlanDto dto);
        // Returns all plans across all insurance types for admin management views
        Task<IEnumerable<PlanDto>> GetAllPlansAsync();
        // Returns plans filtered by a specific insurance type; used in the customer plan selection page
        Task<IEnumerable<PlanDto>> GetPlansByInsuranceTypeAsync(string insuranceTypeId);
        // Returns a single plan by ID with its parent insurance type name
        Task<PlanDto?> GetPlanByIdAsync(string id);
        // Deletes a plan; may fail if active policies reference it
        Task<bool> DeletePlanAsync(string id);
    }
}
