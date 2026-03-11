// Service contract for claim submission, retrieval, status updates, and document upload.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IClaimService
    {
        // Files a new claim against a policy with supporting document uploads
        Task<ClaimDto> FileClaimAsync(CreateClaimDto dto, string policyId, IEnumerable<Microsoft.AspNetCore.Http.IFormFile> files);
        // Transitions a claim to a new status (e.g., UnderReview, Approved, Settled) with optional remarks
        Task<ClaimDto> UpdateClaimStatusAsync(string claimId, UpdateClaimStatusDto dto, string officerId);
        // Returns all claims filed under a specific policy
        Task<IEnumerable<ClaimDto>> GetClaimsByPolicyAsync(string policyId);
        // Returns a single claim by ID with full details, history, and documents
        Task<ClaimDto?> GetClaimByIdAsync(string id);
        // Returns all claims assigned to a specific claims officer
        Task<IEnumerable<ClaimDto>> GetClaimsByOfficerAsync(string officerId);
        // Returns every claim in the system (admin view)
        Task<IEnumerable<ClaimDto>> GetAllClaimsAsync();
    }
}
