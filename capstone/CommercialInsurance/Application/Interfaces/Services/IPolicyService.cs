// Service contract for the full policy lifecycle: quotation, issuance, renewal, and agent assignment.
using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services
{
    public interface IPolicyService
    {
        // Creates a new policy or upgrades a quote to PendingReview; saves uploaded documents
        Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto dto, string createdByUserId, IEnumerable<IFormFile> documents, string? agentId = null);
        // Returns all policies owned by a specific customer
        Task<IEnumerable<PolicyDto>> GetPoliciesByUserAsync(string userId);
        // Returns all policies assigned to a specific agent
        Task<IEnumerable<PolicyDto>> GetPoliciesByAgentAsync(string agentId);
        // Returns a single policy with full detail (documents, agent name, remaining coverage)
        Task<PolicyDto?> GetPolicyByIdAsync(string id);
        // Returns every policy in the system (admin view)
        Task<IEnumerable<PolicyDto>> GetAllPoliciesAsync();
        // Runs the premium calculation engine using risk multipliers and business profile data
        Task<PremiumCalculationDto> CalculatePremiumAsync(PremiumCalculationRequestDto request);
        // Transitions a PendingReview policy to Approved status
        Task<PolicyDto> ApprovePolicyAsync(string policyId);
        // Transitions a PendingReview policy to Rejected with a reason
        Task<PolicyDto> RejectPolicyAsync(string policyId, RejectPolicyDto dto);
        // Activates a policy by setting status to Active and generating a POL- policy number
        Task<PolicyDto> IssuePolicyAsync(string policyId);
        // Flips the AutoRenew flag on a policy
        Task<PolicyDto> ToggleAutoRenewAsync(string policyId);
        // Creates a renewal policy linked to the original, handling early and lapsed renewal logic
        Task<PolicyDto> RenewPolicyAsync(string policyId);
        // Cascade-deletes a policy and all related payments, claims, documents, and history logs
        Task<bool> DeletePolicyAsync(string policyId);
    }
}
