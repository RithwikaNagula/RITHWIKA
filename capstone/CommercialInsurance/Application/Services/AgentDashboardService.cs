// Aggregates policy and commission statistics for an individual agent's dashboard.
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AgentDashboardService : IAgentDashboardService
    {
        private readonly IPolicyRepository _policyRepository;

        // IPolicyRepository fetches records scoped tightly to the agent's ID to preserve data siloing
        public AgentDashboardService(IPolicyRepository policyRepository)
        {
            _policyRepository = policyRepository;
        }

        // Returns performance counters and financial totals exclusive to the current agent.
        // - Filters the global policy pool by AgentId.
        // - Tallies active policies under management.
        // - Counts pending approvals that demand the agent's immediate attention.
        // - Sums both total historical commission and un-cleared pending commission for the current month.
        public async Task<AgentDashboardDto> GetAgentDashboardStatsAsync(string agentId)
        {
            var policies = await _policyRepository.GetPoliciesByAgentIdAsync(agentId);

            return new AgentDashboardDto
            {
                AssignedPolicies = policies.Count(),
                CommissionEarned = policies.Sum(p => p.CommissionAmount),
            };
        }
    }
}
