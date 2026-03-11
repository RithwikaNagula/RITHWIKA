// DTOs for analytics data returned to the admin dashboard: revenue summaries, claim distributions, and user growth timeseries.
using System.Collections.Generic;

namespace Application.DTOs
{
    // Carries per-agent KPIs displayed in the admin analytics agent comparison table
    public class AgentPerformanceDto
    {
        public string AgentName { get; set; } = string.Empty;
        public decimal TotalCommission { get; set; }
    }

    // Carries per-officer KPIs: total settlements handled by each claims officer
    public class ClaimsPerformanceDto
    {
        public string OfficerName { get; set; } = string.Empty;
        public int TotalSettlements { get; set; }
    }

    // Aggregated claims statistics: approval rate, total counts, and officer-level breakdowns
    public class ClaimsAnalyticsDto
    {
        public List<ClaimsPerformanceDto> OfficerPerformances { get; set; } = new();
        public decimal ApprovalRate { get; set; }
        public int TotalClaims { get; set; }
        public int ApprovedClaims { get; set; }
    }

    // Revenue time-series data: premiums collected, settlements paid, net revenue, and period-over-period growth
    public class RevenueAnalyticsDto
    {
        public decimal TotalPremiumCollected { get; set; }
        public decimal TotalSettlementsPaid { get; set; }
        public decimal NetRevenue { get; set; }
        public int TotalPolicies { get; set; }
        public int TotalClaims { get; set; }
        public decimal CurrentPeriodRevenue { get; set; }
        public decimal PreviousPeriodRevenue { get; set; }
        public decimal GrowthPercentage { get; set; }
        public Dictionary<string, decimal> RevenueTrend { get; set; } = new();
    }
}
