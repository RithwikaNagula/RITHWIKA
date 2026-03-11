// Repository contract for policy queries: by user, by agent, by status, and expiring-soon filtering.
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IPolicyRepository : IGenericRepository<Policy>
    {
        // Returns all policies owned by a specific customer, eagerly loading Plan and Agent
        Task<IEnumerable<Policy>> GetPoliciesByUserIdAsync(string userId);
        // Returns all policies managed by a specific agent
        Task<IEnumerable<Policy>> GetPoliciesByAgentIdAsync(string agentId);
        // Looks up a policy by its human-readable POL-XXXXXXXX number
        Task<Policy?> GetPolicyByNumberAsync(string policyNumber);
    }
}
