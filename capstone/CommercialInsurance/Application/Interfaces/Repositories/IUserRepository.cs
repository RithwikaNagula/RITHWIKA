// Repository contract for user lookups: find by email, by ID, and by role.
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        // Finds a user by their email address; used during login and duplicate-email checks
        Task<User?> GetByEmailAsync(string email);
    }
}
