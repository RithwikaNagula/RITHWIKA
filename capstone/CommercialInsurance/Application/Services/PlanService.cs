// Manages the coverage tiers nested under an Insurance Type; enforces plan caps and premium defaults.
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _planRepository;
        private readonly IInsuranceTypeRepository _insuranceTypeRepository;

        // Validates constraints between the overarching Insurance Type and specific Plan offerings
        public PlanService(IPlanRepository planRepository, IInsuranceTypeRepository insuranceTypeRepository)
        {
            _planRepository = planRepository;
            _insuranceTypeRepository = insuranceTypeRepository;
        }

        // Creates a new tier (e.g., Cyber Security - Silver Level).
        // It validates that the parent insurance category actually exists first.
        public async Task<PlanDto> CreatePlanAsync(CreatePlanDto dto)
        {
            var type = await _insuranceTypeRepository.GetByIdAsync(dto.InsuranceTypeId);
            if (type == null) throw new InvalidOperationException("Insurance Type not found.");

            var plan = new Plan
            {
                Id = await GenerateNextId(),
                InsuranceTypeId = dto.InsuranceTypeId,
                PlanName = dto.PlanName,
                Description = dto.Description,
                BasePremium = dto.BasePremium,
                MinCoverageAmount = dto.MinCoverageAmount,
                MaxCoverageAmount = dto.MaxCoverageAmount,
                DurationInMonths = dto.DurationInMonths,
                ImageUrl = dto.ImageUrl
            };

            await _planRepository.AddAsync(plan);
            return MapToDto(plan);
        }

        // Retrieves every plan across the whole platform for the public-facing 'Browse Coverages' page
        public async Task<IEnumerable<PlanDto>> GetAllPlansAsync()
        {
            var plans = await _planRepository.GetAllAsync();
            var types = await _insuranceTypeRepository.GetAllAsync();
            var typeMap = types.ToDictionary(t => t.Id, t => t.TypeName);

            return plans.Select(p =>
            {
                var dto = MapToDto(p);
                if (typeMap.TryGetValue(p.InsuranceTypeId, out var typeName))
                {
                    dto.InsuranceTypeName = typeName;
                }
                return dto;
            });
        }

        // Yields the essential coverage boundary info mapping limits to specific quotes
        public async Task<PlanDto?> GetPlanByIdAsync(string id)
        {
            var plan = await _planRepository.GetByIdAsync(id);
            if (plan == null) return null;
            return MapToDto(plan);
        }

        // Restricts discovery to plans under a single category flag, crucial when users start quotes.
        public async Task<IEnumerable<PlanDto>> GetPlansByInsuranceTypeAsync(string insuranceTypeId)
        {
            var plans = await _planRepository.FindAsync(p => p.InsuranceTypeId == insuranceTypeId);
            var type = await _insuranceTypeRepository.GetByIdAsync(insuranceTypeId);

            return plans.Select(p =>
            {
                var dto = MapToDto(p);
                dto.InsuranceTypeName = type?.TypeName ?? "General";
                return dto;
            });
        }

        // Completely removes the plan from the database.
        // Fails via SQL constraint violation naturally if active quotes/policies still link to it.
        public async Task<bool> DeletePlanAsync(string id)
        {
            var plan = await _planRepository.GetByIdAsync(id);
            if (plan == null) return false;

            await _planRepository.DeleteAsync(plan);
            return true;
        }

        // Ensures pseudo-logical string tracking across the DB.
        private async Task<string> GenerateNextId()
        {
            var all = await _planRepository.GetAllAsync();
            var count = all.Count();
            return $"pln{count + 1}";
        }

        // Transformer logic bridging Database representations to safely serializable JSON shapes
        private PlanDto MapToDto(Plan plan)
        {
            return new PlanDto
            {
                Id = plan.Id,
                InsuranceTypeId = plan.InsuranceTypeId,
                PlanName = plan.PlanName,
                Description = plan.Description,
                BasePremium = plan.BasePremium,
                MinCoverageAmount = plan.MinCoverageAmount,
                MaxCoverageAmount = plan.MaxCoverageAmount,
                DurationInMonths = plan.DurationInMonths,
                IsActive = plan.IsActive,
                ImageUrl = plan.ImageUrl,
                CreatedAt = plan.CreatedAt
            };
        }
    }
}
