// Repository contract for claim queries: by policy, by assigned officer, and pending review lists.
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IClaimRepository : IGenericRepository<Claim>
    {
        // Returns all claims filed under a specific policy
        Task<IEnumerable<Claim>> GetClaimsByPolicyIdAsync(string policyId);
        // Returns all claims assigned to a specific claims officer for review
        Task<IEnumerable<Claim>> GetClaimsByOfficerIdAsync(string officerId);
        // Looks up a claim by its human-readable CLM-XXXXXXXX number
        Task<Claim?> GetClaimByNumberAsync(string claimNumber);
    }
}
