// Provides core functionality and structures for the application.
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IPlanRepository : IGenericRepository<Plan>
    {
        Task<IEnumerable<Plan>> GetPlansByInsuranceTypeAsync(string insuranceTypeId);
        Task<IEnumerable<Plan>> GetActivePlansAsync();
    }
}
