// Provides core functionality and structures for the application.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardDto> GetDashboardStatsAsync();
        Task<AdminAnalyticsDto> GetAnalyticsAsync();
    }
}
