// Service contract for admin dashboard aggregates: user counts, policy totals, revenue, and recent activity.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IAdminDashboardService
    {
        // Returns aggregated KPIs (user counts, policy totals, revenue) for the admin dashboard landing page
        Task<AdminDashboardDto> GetDashboardStatsAsync();
        // Returns chart-ready analytics: policies by type, claims by status, revenue over time
        Task<AdminAnalyticsDto> GetAnalyticsAsync();
    }
}
