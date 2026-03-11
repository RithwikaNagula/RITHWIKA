// Concrete repository implementations for User, Policy, Claim, Payment, Plan, InsuranceType, BusinessProfile, and Notification with domain-specific queries.
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    // Extends GenericRepository<User> with email lookups and eager-loaded agent/officer navigation
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(InsuranceDbContext context) : base(context) { }

        // Looks up a user by email and eagerly loads their agent and officer assignments to avoid N+1 queries
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Include(u => u.AssignedAgent)
                .Include(u => u.AssignedClaimsOfficer)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        // Overrides base GetById to include assigned agent and officer names used by UserDto mapping
        public override async Task<User?> GetByIdAsync(string id)
        {
            return await _dbSet
                .Include(u => u.AssignedAgent)
                .Include(u => u.AssignedClaimsOfficer)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }

    // Extends GenericRepository<Plan> with type-filtered and active-only queries
    public class PlanRepository : GenericRepository<Plan>, IPlanRepository
    {
        public PlanRepository(InsuranceDbContext context) : base(context) { }

        // Returns only active plans under a specific insurance type; includes InsuranceType for display names
        public async Task<IEnumerable<Plan>> GetPlansByInsuranceTypeAsync(string insuranceTypeId)
        {
            return await _dbSet
                .Include(p => p.InsuranceType)
                .AsNoTracking()
                .Where(p => p.InsuranceTypeId == insuranceTypeId && p.IsActive)
                .ToListAsync();
        }

        // Returns all active plans across all insurance types for admin management views
        public async Task<IEnumerable<Plan>> GetActivePlansAsync()
        {
            return await _dbSet
                .Include(p => p.InsuranceType)
                .AsNoTracking()
                .Where(p => p.IsActive)
                .ToListAsync();
        }
    }

    // Extends GenericRepository<Policy> with customer, agent, and policy-number queries
    public class PolicyRepository : GenericRepository<Policy>, IPolicyRepository
    {
        public PolicyRepository(InsuranceDbContext context) : base(context) { }

        // Returns all policies belonging to a specific customer
        public async Task<IEnumerable<Policy>> GetPoliciesByUserIdAsync(string userId)
        {
            return await _dbSet.Where(p => p.UserId == userId).ToListAsync();
        }

        // Returns all policies managed by a specific agent
        public async Task<IEnumerable<Policy>> GetPoliciesByAgentIdAsync(string agentId)
        {
            return await _dbSet.Where(p => p.AgentId == agentId).ToListAsync();
        }

        // Looks up a policy by its human-readable POL-XXXXXXXX number
        public async Task<Policy?> GetPolicyByNumberAsync(string policyNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber);
        }
    }

    // Extends GenericRepository<Claim> with policy-based, officer-based, and claim-number queries
    public class ClaimRepository : GenericRepository<Claim>, IClaimRepository
    {
        public ClaimRepository(InsuranceDbContext context) : base(context) { }

        // Returns all claims filed for a specific policy
        public async Task<IEnumerable<Claim>> GetClaimsByPolicyIdAsync(string policyId)
        {
            return await _dbSet.Where(c => c.PolicyId == policyId).ToListAsync();
        }

        // Returns all claims assigned to a claims officer for review
        public async Task<IEnumerable<Claim>> GetClaimsByOfficerIdAsync(string officerId)
        {
            return await _dbSet.Where(c => c.ClaimsOfficerId == officerId).ToListAsync();
        }

        // Looks up a claim by its CLM-XXXXXXXX number
        public async Task<Claim?> GetClaimByNumberAsync(string claimNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.ClaimNumber == claimNumber);
        }
    }

    // Extends GenericRepository<Payment> with policy-scoped payment queries
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(InsuranceDbContext context) : base(context) { }

        // Returns all payment records for a given policy
        public async Task<IEnumerable<Payment>> GetPaymentsByPolicyIdAsync(string policyId)
        {
            return await _dbSet.Where(p => p.PolicyId == policyId).ToListAsync();
        }
    }

    // Extends GenericRepository<BusinessProfile> with user-scoped profile queries
    public class BusinessProfileRepository : GenericRepository<BusinessProfile>, IBusinessProfileRepository
    {
        public BusinessProfileRepository(InsuranceDbContext context) : base(context) { }

        // Returns the first (primary) profile for a customer; used during quick eligibility checks
        public async Task<BusinessProfile?> GetByUserIdAsync(string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(b => b.UserId == userId);
        }

        // Returns all profiles owned by a customer (customers may have multiple business entities)
        public async Task<IEnumerable<BusinessProfile>> GetProfilesByUserIdAsync(string userId)
        {
            return await _dbSet.Where(b => b.UserId == userId).ToListAsync();
        }
    }

    // Insurance type repository; relies entirely on base GenericRepository CRUD operations
    public class InsuranceTypeRepository : GenericRepository<InsuranceType>, IInsuranceTypeRepository
    {
        public InsuranceTypeRepository(InsuranceDbContext context) : base(context) { }
    }

    // Notification repository; relies entirely on base GenericRepository CRUD operations
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(InsuranceDbContext context) : base(context) { }
    }
}
