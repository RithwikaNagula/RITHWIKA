// Service contract for agent dashboard data: assigned policies, commission totals, and customer counts.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IAgentDashboardService
    {
        // Returns assigned policy count, commission earned, and active customer count for the given agent
        Task<AgentDashboardDto> GetAgentDashboardStatsAsync(string agentId);
    }
}
