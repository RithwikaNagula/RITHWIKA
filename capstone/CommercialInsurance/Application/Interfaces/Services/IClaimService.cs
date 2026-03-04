// Provides core functionality and structures for the application.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IClaimService
    {
        Task<ClaimDto> FileClaimAsync(CreateClaimDto dto, string policyId, IEnumerable<Microsoft.AspNetCore.Http.IFormFile> files);
        Task<ClaimDto> UpdateClaimStatusAsync(string claimId, UpdateClaimStatusDto dto, string officerId);
        Task<IEnumerable<ClaimDto>> GetClaimsByPolicyAsync(string policyId);
        Task<ClaimDto?> GetClaimByIdAsync(string id);
        Task<IEnumerable<ClaimDto>> GetClaimsByOfficerAsync(string officerId);
        Task<IEnumerable<ClaimDto>> GetAllClaimsAsync();
    }
}
