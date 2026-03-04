// Provides core functionality and structures for the application.
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;

namespace Application.Services
{
    public class AgentDashboardService : IAgentDashboardService
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IUserRepository _userRepository;

        public AgentDashboardService(IPolicyRepository policyRepository, IUserRepository userRepository)
        {
            _policyRepository = policyRepository;
            _userRepository = userRepository;
        }

        public async Task<AgentDashboardDto> GetAgentDashboardStatsAsync(string agentId)
        {
            var agent = await _userRepository.GetByIdAsync(agentId);
            if (agent == null || agent.Role != UserRole.Agent)
                throw new InvalidOperationException("User is not a valid Agent.");

            var allAgentPolicies = await _policyRepository.GetPoliciesByAgentIdAsync(agentId);

            var totalPolicies = allAgentPolicies.Count();
            var activePolicies = allAgentPolicies.Count(p => p.Status == PolicyStatus.Active);

            // Commission logic: assuming a flat 10% commission on the PremiumAmount of Active policies, or you can calculate it based on something else
            decimal totalCommission = allAgentPolicies
                .Where(p => p.Status == PolicyStatus.Active)
                .Sum(p => p.PremiumAmount * 0.10m);

            // Active customers = distinct users from active policies
            var activeCustomers = allAgentPolicies
                .Where(p => p.Status == PolicyStatus.Active)
                .Select(p => p.UserId)
                .Distinct()
                .Count();

            // Conversion rate: (Active Policies / Total Assigned Policies) * 100
            decimal conversionRate = totalPolicies > 0
                ? ((decimal)activePolicies / totalPolicies) * 100
                : 0;

            return new AgentDashboardDto
            {
                AssignedPolicies = totalPolicies,
                ConversionRate = Math.Round(conversionRate, 2),
                CommissionEarned = Math.Round(totalCommission, 2),
                ActiveCustomers = activeCustomers
            };
        }
    }
}
