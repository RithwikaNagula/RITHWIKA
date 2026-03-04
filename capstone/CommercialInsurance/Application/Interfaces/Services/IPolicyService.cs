using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services
{
    public interface IPolicyService
    {
        Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto dto, string createdByUserId, IEnumerable<IFormFile> documents, string? agentId = null);
        Task<IEnumerable<PolicyDto>> GetPoliciesByUserAsync(string userId);
        Task<IEnumerable<PolicyDto>> GetPoliciesByAgentAsync(string agentId);
        Task<PolicyDto?> GetPolicyByIdAsync(string id);
        Task<IEnumerable<PolicyDto>> GetAllPoliciesAsync();
        Task<PremiumCalculationDto> CalculatePremiumAsync(PremiumCalculationRequestDto request);
        Task<PolicyDto> ApprovePolicyAsync(string policyId);
        Task<PolicyDto> RejectPolicyAsync(string policyId, RejectPolicyDto dto);
        Task<PolicyDto> IssuePolicyAsync(string policyId);
        Task<PolicyDto> ToggleAutoRenewAsync(string policyId);
        Task<PolicyDto> RenewPolicyAsync(string policyId);
        Task<bool> DeletePolicyAsync(string policyId);
    }
}
