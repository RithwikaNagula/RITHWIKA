// Repository contract for business profile lookups by customer user ID.
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IBusinessProfileRepository : IGenericRepository<BusinessProfile>
    {
        // Returns the first profile for a customer; used during quick eligibility checks
        Task<BusinessProfile?> GetByUserIdAsync(string userId);
        // Returns all profiles owned by a customer (supports multi-business scenarios)
        Task<IEnumerable<BusinessProfile>> GetProfilesByUserIdAsync(string userId);
    }
}
