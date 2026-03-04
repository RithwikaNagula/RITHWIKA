// Provides core functionality and structures for the application.
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using System.Linq;

namespace Application.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPlanRepository _planRepository;
        private readonly IGenericRepository<InsuranceType> _insuranceTypeRepository;

        public AdminDashboardService(
            IUserRepository userRepository,
            IPolicyRepository policyRepository,
            IClaimRepository claimRepository,
            IPaymentRepository paymentRepository,
            IPlanRepository planRepository,
            IGenericRepository<InsuranceType> insuranceTypeRepository)
        {
            _userRepository = userRepository;
            _policyRepository = policyRepository;
            _claimRepository = claimRepository;
            _paymentRepository = paymentRepository;
            _planRepository = planRepository;
            _insuranceTypeRepository = insuranceTypeRepository;
        }

        public async Task<AdminDashboardDto> GetDashboardStatsAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var policies = await _policyRepository.GetAllAsync();
            var claims = await _claimRepository.GetAllAsync();
            var payments = await _paymentRepository.GetAllAsync();

            var totalUsers = users.Count();
            var totalAgents = users.Count(u => u.Role == UserRole.Agent);
            var totalOfficers = users.Count(u => u.Role == UserRole.ClaimsOfficer);

            var totalPolicies = policies.Count();
            var activePolicies = policies.Count(p => p.Status == PolicyStatus.Active);

            var totalClaims = claims.Count();
            var pendingClaims = claims.Count(c => c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.UnderReview);

            var totalRevenue = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);

            return new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                TotalAgents = totalAgents,
                TotalClaimsOfficers = totalOfficers,
                TotalPolicies = totalPolicies,
                TotalClaims = totalClaims,
                TotalRevenue = totalRevenue,
                ActivePolicies = activePolicies,
                PendingClaims = pendingClaims
            };
        }

        public async Task<AdminAnalyticsDto> GetAnalyticsAsync()
        {
            var analytics = new AdminAnalyticsDto();

            var policies = await _policyRepository.GetAllAsync();
            var claims = await _claimRepository.GetAllAsync();
            var payments = await _paymentRepository.GetAllAsync();
            var plans = await _planRepository.GetAllAsync();
            var types = await _insuranceTypeRepository.GetAllAsync();
            var agents = await _userRepository.FindAsync(u => u.Role == UserRole.Agent);

            if (policies != null && plans != null && types != null)
            {
                foreach (var p in policies)
                {
                    var plan = plans.FirstOrDefault(x => x.Id == p.PlanId);
                    var type = types.FirstOrDefault(t => t.Id == plan?.InsuranceTypeId);
                    var typeName = type?.TypeName ?? "Unknown";

                    if (!analytics.PoliciesByType.ContainsKey(typeName))
                        analytics.PoliciesByType[typeName] = 0;
                    analytics.PoliciesByType[typeName]++;
                }
            }

            if (claims != null)
            {
                foreach (var c in claims)
                {
                    var status = c.Status.ToString();
                    if (!analytics.ClaimsByStatus.ContainsKey(status))
                        analytics.ClaimsByStatus[status] = 0;
                    analytics.ClaimsByStatus[status]++;
                }
            }

            if (payments != null)
            {
                foreach (var p in payments.Where(x => x.Status == PaymentStatus.Completed))
                {
                    var monthStr = p.PaymentDate.ToString("MMM yyyy");
                    if (!analytics.RevenueByMonth.ContainsKey(monthStr))
                        analytics.RevenueByMonth[monthStr] = 0;
                    analytics.RevenueByMonth[monthStr] += p.Amount;
                }
            }

            if (agents != null && policies != null)
            {
                foreach (var a in agents)
                {
                    var count = policies.Count(p => p.AgentId == a.Id);
                    analytics.PoliciesByAgent[a.FullName] = count;
                }
            }

            return analytics;
        }
    }
}
