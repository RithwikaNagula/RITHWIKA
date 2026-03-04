// Provides core functionality and structures for the application.
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(InsuranceDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Include(u => u.AssignedAgent)
                .Include(u => u.AssignedClaimsOfficer)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public override async Task<User?> GetByIdAsync(string id)
        {
            return await _dbSet
                .Include(u => u.AssignedAgent)
                .Include(u => u.AssignedClaimsOfficer)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }

    public class PlanRepository : GenericRepository<Plan>, IPlanRepository
    {
        public PlanRepository(InsuranceDbContext context) : base(context) { }

        public async Task<IEnumerable<Plan>> GetPlansByInsuranceTypeAsync(string insuranceTypeId)
        {
            return await _dbSet
                .Include(p => p.InsuranceType)
                .AsNoTracking()
                .Where(p => p.InsuranceTypeId == insuranceTypeId && p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Plan>> GetActivePlansAsync()
        {
            return await _dbSet
                .Include(p => p.InsuranceType)
                .AsNoTracking()
                .Where(p => p.IsActive)
                .ToListAsync();
        }
    }

    public class PolicyRepository : GenericRepository<Policy>, IPolicyRepository
    {
        public PolicyRepository(InsuranceDbContext context) : base(context) { }

        public async Task<IEnumerable<Policy>> GetPoliciesByUserIdAsync(string userId)
        {
            return await _dbSet.Where(p => p.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<Policy>> GetPoliciesByAgentIdAsync(string agentId)
        {
            return await _dbSet.Where(p => p.AgentId == agentId).ToListAsync();
        }

        public async Task<Policy?> GetPolicyByNumberAsync(string policyNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber);
        }
    }

    public class ClaimRepository : GenericRepository<Claim>, IClaimRepository
    {
        public ClaimRepository(InsuranceDbContext context) : base(context) { }

        public async Task<IEnumerable<Claim>> GetClaimsByPolicyIdAsync(string policyId)
        {
            return await _dbSet.Where(c => c.PolicyId == policyId).ToListAsync();
        }

        public async Task<IEnumerable<Claim>> GetClaimsByOfficerIdAsync(string officerId)
        {
            return await _dbSet.Where(c => c.ClaimsOfficerId == officerId).ToListAsync();
        }

        public async Task<Claim?> GetClaimByNumberAsync(string claimNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.ClaimNumber == claimNumber);
        }
    }

    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(InsuranceDbContext context) : base(context) { }

        public async Task<IEnumerable<Payment>> GetPaymentsByPolicyIdAsync(string policyId)
        {
            return await _dbSet.Where(p => p.PolicyId == policyId).ToListAsync();
        }
    }

    public class BusinessProfileRepository : GenericRepository<BusinessProfile>, IBusinessProfileRepository
    {
        public BusinessProfileRepository(InsuranceDbContext context) : base(context) { }

        public async Task<BusinessProfile?> GetByUserIdAsync(string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(b => b.UserId == userId);
        }

        public async Task<IEnumerable<BusinessProfile>> GetProfilesByUserIdAsync(string userId)
        {
            return await _dbSet.Where(b => b.UserId == userId).ToListAsync();
        }
    }

    public class InsuranceTypeRepository : GenericRepository<InsuranceType>, IInsuranceTypeRepository
    {
        public InsuranceTypeRepository(InsuranceDbContext context) : base(context) { }
    }

    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(InsuranceDbContext context) : base(context) { }
    }
}
