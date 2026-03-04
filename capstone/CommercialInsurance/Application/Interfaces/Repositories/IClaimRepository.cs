// Provides core functionality and structures for the application.
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IClaimRepository : IGenericRepository<Claim>
    {
        Task<IEnumerable<Claim>> GetClaimsByPolicyIdAsync(string policyId);
        Task<IEnumerable<Claim>> GetClaimsByOfficerIdAsync(string officerId);
        Task<Claim?> GetClaimByNumberAsync(string claimNumber);
    }
}
