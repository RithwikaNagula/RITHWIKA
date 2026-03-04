using System.Collections.Generic;

namespace Application.DTOs
{
    public class AgentPerformanceDto
    {
        public string AgentName { get; set; } = string.Empty;
        public decimal TotalCommission { get; set; }
    }

    public class ClaimsPerformanceDto
    {
        public string OfficerName { get; set; } = string.Empty;
        public int TotalSettlements { get; set; }
    }

    public class ClaimsAnalyticsDto
    {
        public List<ClaimsPerformanceDto> OfficerPerformances { get; set; } = new();
        public decimal ApprovalRate { get; set; }
        public int TotalClaims { get; set; }
        public int ApprovedClaims { get; set; }
    }

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
