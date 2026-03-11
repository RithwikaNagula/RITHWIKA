// Gathers system-wide aggregated metrics for the admin dashboard overview.
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IClaimRepository _claimRepository;

        // Service aggregation layer — accesses multiple generic and specific repositories
        // to compute high-level statistics without exposing complex EF queries directly downstream.
        public AdminDashboardService(
            IUserRepository userRepository,
            IPolicyRepository policyRepository,
            IPaymentRepository paymentRepository,
            IClaimRepository claimRepository)
        {
            _userRepository = userRepository;
            _policyRepository = policyRepository;
            _paymentRepository = paymentRepository;
            _claimRepository = claimRepository;
        }

        // Assembles the primary KPI metrics for the admin interface:
        // - Counts users by distinct roles (Customer, Agent, ClaimsOfficer).
        // - Sums up all revenue across the entire platform.
        // - Totals the number of active, in-force insurance policies.
        public async Task<AdminDashboardDto> GetDashboardStatsAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var policies = await _policyRepository.GetAllAsync();
            var payments = await _paymentRepository.GetAllAsync();
            var claims = await _claimRepository.GetAllAsync();

            return new AdminDashboardDto
            {
                TotalUsers = users.Count(),
                TotalAgents = users.Count(u => u.Role == Domain.Enums.UserRole.Agent),
                TotalClaimsOfficers = users.Count(u => u.Role == Domain.Enums.UserRole.ClaimsOfficer),
                TotalPolicies = policies.Count(),
                TotalRevenue = payments.Sum(p => p.Amount),
                ActivePolicies = policies.Count(p => p.Status == Domain.Enums.PolicyStatus.Active),
                TotalClaims = claims.Count(),
                PendingClaims = claims.Count(c => c.Status == Domain.Enums.ClaimStatus.Submitted)
            };
        }

        public async Task<AdminAnalyticsDto> GetAnalyticsAsync()
        {
            return new AdminAnalyticsDto();
        }
    }
}
