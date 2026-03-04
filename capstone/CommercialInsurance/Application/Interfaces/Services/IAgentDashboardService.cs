// Provides core functionality and structures for the application.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IAgentDashboardService
    {
        Task<AgentDashboardDto> GetAgentDashboardStatsAsync(string agentId);
    }
}
