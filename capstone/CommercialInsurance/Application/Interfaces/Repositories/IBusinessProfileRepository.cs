// Provides core functionality and structures for the application.
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IBusinessProfileRepository : IGenericRepository<BusinessProfile>
    {
        Task<BusinessProfile?> GetByUserIdAsync(string userId);
        Task<IEnumerable<BusinessProfile>> GetProfilesByUserIdAsync(string userId);
    }
}
