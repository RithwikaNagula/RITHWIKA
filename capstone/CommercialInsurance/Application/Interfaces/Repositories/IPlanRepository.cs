// Repository contract for fetching plans by insurance type.
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IPlanRepository : IGenericRepository<Plan>
    {
        // Returns plans filtered by their parent insurance type; used in the plan selection page
        Task<IEnumerable<Plan>> GetPlansByInsuranceTypeAsync(string insuranceTypeId);
        // Returns only active plans (not soft-deleted) across all insurance types
        Task<IEnumerable<Plan>> GetActivePlansAsync();
    }
}
