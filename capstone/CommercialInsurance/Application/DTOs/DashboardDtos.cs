// DTOs for role-specific dashboard summaries: admin metrics, agent performance stats, and claims officer worklist counts.
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    // Outbound KPIs for the admin landing dashboard: total users, policies, claims, and revenue
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalAgents { get; set; }
        public int TotalClaimsOfficers { get; set; }
        public int TotalPolicies { get; set; }
        public int TotalClaims { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActivePolicies { get; set; }
        public int PendingClaims { get; set; }
    }

    // Chart-ready aggregates for admin analytics: policies grouped by type, claims by status, revenue by month
    public class AdminAnalyticsDto
    {
        public Dictionary<string, int> PoliciesByType { get; set; } = new();
        public Dictionary<string, int> ClaimsByStatus { get; set; } = new();
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
        public Dictionary<string, int> PoliciesByAgent { get; set; } = new();
    }

    // Outbound KPIs for the agent landing dashboard: assigned policies, commission, and customer counts
    public class AgentDashboardDto
    {
        public int AssignedPolicies { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal CommissionEarned { get; set; }
        public int ActiveCustomers { get; set; }
    }
}
