// Provides core functionality and structures for the application.
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IPolicyRepository : IGenericRepository<Policy>
    {
        Task<IEnumerable<Policy>> GetPoliciesByUserIdAsync(string userId);
        Task<IEnumerable<Policy>> GetPoliciesByAgentIdAsync(string agentId);
        Task<Policy?> GetPolicyByNumberAsync(string policyNumber);
    }
}
