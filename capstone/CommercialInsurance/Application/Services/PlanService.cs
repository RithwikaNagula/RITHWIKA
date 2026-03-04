// Provides core functionality and structures for the application.
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _planRepository;

        public PlanService(IPlanRepository planRepository)
        {
            _planRepository = planRepository;
        }

        public async Task<PlanDto> CreatePlanAsync(CreatePlanDto dto)
        {
            var id = await GenerateNextId();
            var plan = new Plan
            {
                Id = id,
                PlanName = dto.PlanName,
                Description = dto.Description,
                MinCoverageAmount = dto.MinCoverageAmount,
                MaxCoverageAmount = dto.MaxCoverageAmount,
                BasePremium = dto.BasePremium,
                DurationInMonths = dto.DurationInMonths,
                InsuranceTypeId = dto.InsuranceTypeId,
                IsActive = true
            };

            await _planRepository.AddAsync(plan);

            return new PlanDto
            {
                Id = plan.Id,
                PlanName = plan.PlanName,
                Description = plan.Description,
                MinCoverageAmount = plan.MinCoverageAmount,
                MaxCoverageAmount = plan.MaxCoverageAmount,
                BasePremium = plan.BasePremium,
                DurationInMonths = plan.DurationInMonths,
                InsuranceTypeId = plan.InsuranceTypeId,
                IsActive = plan.IsActive,
                CreatedAt = plan.CreatedAt
            };
        }

        public async Task<IEnumerable<PlanDto>> GetAllPlansAsync()
        {
            var plans = await _planRepository.GetActivePlansAsync();
            return plans.Select(p => new PlanDto
            {
                Id = p.Id,
                PlanName = p.PlanName,
                Description = p.Description,
                MinCoverageAmount = p.MinCoverageAmount,
                MaxCoverageAmount = p.MaxCoverageAmount,
                BasePremium = p.BasePremium,
                DurationInMonths = p.DurationInMonths,
                InsuranceTypeId = p.InsuranceTypeId,
                InsuranceTypeName = p.InsuranceType?.TypeName ?? "",
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            });
        }

        public async Task<IEnumerable<PlanDto>> GetPlansByInsuranceTypeAsync(string insuranceTypeId)
        {
            var plans = await _planRepository.GetPlansByInsuranceTypeAsync(insuranceTypeId);
            return plans.Select(p => new PlanDto
            {
                Id = p.Id,
                PlanName = p.PlanName,
                Description = p.Description,
                MinCoverageAmount = p.MinCoverageAmount,
                MaxCoverageAmount = p.MaxCoverageAmount,
                BasePremium = p.BasePremium,
                DurationInMonths = p.DurationInMonths,
                InsuranceTypeId = p.InsuranceTypeId,
                InsuranceTypeName = p.InsuranceType?.TypeName ?? "",
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            });
        }

        public async Task<PlanDto?> GetPlanByIdAsync(string id)
        {
            var plan = await _planRepository.GetByIdAsync(id);
            if (plan == null) return null;

            return new PlanDto
            {
                Id = plan.Id,
                PlanName = plan.PlanName,
                Description = plan.Description,
                MinCoverageAmount = plan.MinCoverageAmount,
                MaxCoverageAmount = plan.MaxCoverageAmount,
                BasePremium = plan.BasePremium,
                DurationInMonths = plan.DurationInMonths,
                InsuranceTypeId = plan.InsuranceTypeId,
                IsActive = plan.IsActive,
                CreatedAt = plan.CreatedAt
            };
        }

        public async Task<bool> DeletePlanAsync(string id)
        {
            var plan = await _planRepository.GetByIdAsync(id);
            if (plan == null) return false;

            await _planRepository.DeleteAsync(plan);
            return true;
        }

        private async Task<string> GenerateNextId()
        {
            var all = await _planRepository.GetAllAsync();
            var count = all.Count();
            return $"pln{count + 1}";
        }
    }
}
