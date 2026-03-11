// Service contract for system analytics queries used by admin charts and reports.
using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IAnalyticsService
    {
        // Returns per-agent KPIs: policies issued, total premium, and commission earned
        Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync();
        // Returns claims breakdown: approval rate, officer performance, and total counts
        Task<ClaimsAnalyticsDto> GetClaimsPerformanceAsync();
        // Returns time-series revenue data for the specified period (monthly or yearly)
        Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(string period);
    }
}
