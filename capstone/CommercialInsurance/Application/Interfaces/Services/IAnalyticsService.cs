using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IAnalyticsService
    {
        Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync();
        Task<ClaimsAnalyticsDto> GetClaimsPerformanceAsync();
        Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(string period);
    }
}
