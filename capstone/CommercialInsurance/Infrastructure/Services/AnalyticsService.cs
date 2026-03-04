using Application.DTOs;
using Application.Interfaces.Services;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly InsuranceDbContext _context;

        public AnalyticsService(InsuranceDbContext context)
        {
            _context = context;
        }

        public async Task<List<AgentPerformanceDto>> GetAgentPerformanceAsync()
        {
            var agentPerformance = await _context.Policies
                .Where(p => p.AgentId != null)
                .GroupBy(p => p.AgentId)
                .Select(g => new
                {
                    AgentId = g.Key,
                    TotalCommission = g.Sum(x => x.CommissionAmount)
                })
                .ToListAsync();

            var users = await _context.Users.Where(u => u.Role == UserRole.Agent).ToDictionaryAsync(u => u.Id, u => u.FullName);

            var dtos = new List<AgentPerformanceDto>();
            foreach (var item in agentPerformance)
            {
                if (item.AgentId != null && users.TryGetValue(item.AgentId, out var agentName))
                {
                    dtos.Add(new AgentPerformanceDto
                    {
                        AgentName = agentName,
                        TotalCommission = item.TotalCommission
                    });
                }
            }

            return dtos.OrderByDescending(d => d.TotalCommission).ToList();
        }

        public async Task<ClaimsAnalyticsDto> GetClaimsPerformanceAsync()
        {
            var allClaims = await _context.Claims.ToListAsync();
            var totalClaims = allClaims.Count;
            var approvedStatuses = new[] { ClaimStatus.Approved, ClaimStatus.Settled };
            var approvedClaims = allClaims.Count(c => approvedStatuses.Contains(c.Status));
            var approvalRate = totalClaims > 0 ? Math.Round((decimal)approvedClaims / totalClaims * 100, 2) : 0;

            var settlementsByOfficer = allClaims
                .Where(c => c.ClaimsOfficerId != null && c.Status == ClaimStatus.Settled)
                .GroupBy(c => c.ClaimsOfficerId)
                .ToDictionary(g => g.Key!, g => g.Count());

            var officerIds = settlementsByOfficer.Keys.ToList();
            var officers = await _context.Users.Where(u => officerIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.FullName);

            var officerPerformances = new List<ClaimsPerformanceDto>();
            foreach (var so in settlementsByOfficer)
            {
                officers.TryGetValue(so.Key, out var officerName);
                officerPerformances.Add(new ClaimsPerformanceDto
                {
                    OfficerName = officerName ?? "Unknown",
                    TotalSettlements = so.Value
                });
            }

            return new ClaimsAnalyticsDto
            {
                ApprovalRate = approvalRate,
                TotalClaims = totalClaims,
                ApprovedClaims = approvedClaims,
                OfficerPerformances = officerPerformances.OrderByDescending(o => o.TotalSettlements).ToList()
            };
        }

        public async Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(string period)
        {
            var isYearly = period?.ToLower() == "yearly";

            var allPayments = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .ToListAsync();

            var allClaims = await _context.Claims
                .Where(c => c.Status == ClaimStatus.Settled)
                .ToListAsync();

            var totalPremiumCollected = allPayments.Sum(p => p.Amount);
            var totalSettlementsPaid = allClaims.Sum(c => c.ClaimAmount);
            var netRevenue = totalPremiumCollected - totalSettlementsPaid;

            var revenueTrend = new Dictionary<string, decimal>();

            DateTime now = DateTime.UtcNow;
            DateTime currentPeriodStart, previousPeriodStart, previousPeriodEnd;
            
            if (isYearly)
            {
                currentPeriodStart = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                previousPeriodStart = new DateTime(now.Year - 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                previousPeriodEnd = new DateTime(now.Year - 1, 12, 31, 23, 59, 59, DateTimeKind.Utc);

                // build 5 year trend
                for (int i = 4; i >= 0; i--)
                {
                    var year = now.Year - i;
                    var yearPremium = allPayments.Where(p => p.PaymentDate.Year == year).Sum(p => p.Amount);
                    var yearSettlement = allClaims.Where(c => c.ResolvedAt.HasValue && c.ResolvedAt.Value.Year == year).Sum(c => c.ClaimAmount);
                    revenueTrend.Add(year.ToString(), yearPremium - yearSettlement);
                }
            }
            else
            {
                currentPeriodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var lastMonth = now.AddMonths(-1);
                previousPeriodStart = new DateTime(lastMonth.Year, lastMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                previousPeriodEnd = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(-1);

                // build 6 month trend
                for (int i = 5; i >= 0; i--)
                {
                    var m = now.AddMonths(-i);
                    var monthKey = m.ToString("MMM yyyy");
                    var monthPremium = allPayments.Where(p => p.PaymentDate.Year == m.Year && p.PaymentDate.Month == m.Month).Sum(p => p.Amount);
                    var monthSettlement = allClaims.Where(c => c.ResolvedAt.HasValue && c.ResolvedAt.Value.Year == m.Year && c.ResolvedAt.Value.Month == m.Month).Sum(c => c.ClaimAmount);
                    revenueTrend.Add(monthKey, monthPremium - monthSettlement);
                }
            }

            var currentPeriodPremium = allPayments.Where(p => p.PaymentDate >= currentPeriodStart).Sum(p => p.Amount);
            var currentPeriodSettlement = allClaims.Where(c => c.ResolvedAt.HasValue && c.ResolvedAt.Value >= currentPeriodStart).Sum(c => c.ClaimAmount);
            var curRev = currentPeriodPremium - currentPeriodSettlement;

            var previousPeriodPremium = allPayments.Where(p => p.PaymentDate >= previousPeriodStart && p.PaymentDate <= previousPeriodEnd).Sum(p => p.Amount);
            var previousPeriodSettlement = allClaims.Where(c => c.ResolvedAt.HasValue && c.ResolvedAt.Value >= previousPeriodStart && c.ResolvedAt.Value <= previousPeriodEnd).Sum(c => c.ClaimAmount);
            var prevRev = previousPeriodPremium - previousPeriodSettlement;

            var growth = prevRev == 0 ? (curRev > 0 ? 100 : 0) : Math.Round(((curRev - prevRev) / Math.Abs(prevRev)) * 100, 2);

            var totalPolicies = await _context.Policies.CountAsync();
            var totalClaims = await _context.Claims.CountAsync();

            return new RevenueAnalyticsDto
            {
                TotalPremiumCollected = totalPremiumCollected,
                TotalSettlementsPaid = totalSettlementsPaid,
                NetRevenue = netRevenue,
                TotalPolicies = totalPolicies,
                TotalClaims = totalClaims,
                CurrentPeriodRevenue = curRev,
                PreviousPeriodRevenue = prevRev,
                GrowthPercentage = growth,
                RevenueTrend = revenueTrend
            };
        }
    }
}
