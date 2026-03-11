// Handles creation and update of business profiles; each customer may have one profile required for policy purchase.
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class BusinessProfileService : IBusinessProfileService
    {
        private readonly IBusinessProfileRepository _profileRepository;

        // Abstracted repository prevents leaking data context complexities through the domain
        public BusinessProfileService(IBusinessProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        // Fetches the first business profile found for a user ID. 
        // Used extensively to verify if a user has fulfilled the KYC prerequisite prior to getting quotes.
        public async Task<BusinessProfileDto?> GetByUserIdAsync(string userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null) return null;

            return MapToDto(profile);
        }

        // Fetches a profile distinctly by the profile's own ID, useful if the system expands to multi-profile.
        public async Task<BusinessProfileDto?> GetByIdAsync(string id)
        {
            var profile = await _profileRepository.GetByIdAsync(id);
            if (profile == null) return null;

            return MapToDto(profile);
        }

        // Retrieves all profiles directly owned by the passed user context.
        public async Task<IEnumerable<BusinessProfileDto>> GetProfilesByUserIdAsync(string userId)
        {
            var profiles = await _profileRepository.GetProfilesByUserIdAsync(userId);
            return profiles.Select(MapToDto);
        }

        // Simple sequencer ensuring primary key uniqueness (bpro1, bpro2).
        private async Task<string> GenerateNextId()
        {
            var all = await _profileRepository.GetAllAsync();
            var count = all.Count();
            return $"bpro{count + 1}";
        }

        // Implements standard IDempotent Upsert logic: tests if the combination of UserID and BusinessName
        // exists. If not found, provisions a complete new profile. If found, upgrades the pre-existing row 
        // cleanly without duplicating.
        public async Task<BusinessProfileDto> UpsertProfileAsync(string userId, CreateBusinessProfileDto dto)
        {
            // Support multiple profiles: check if a profile with SAME BusinessName exists for this user
            var profiles = await _profileRepository.GetProfilesByUserIdAsync(userId);
            var profile = profiles.FirstOrDefault(p => p.BusinessName.Equals(dto.BusinessName, StringComparison.OrdinalIgnoreCase));

            if (profile == null)
            {
                // Insertion branch triggering when no exact string match is found
                profile = new BusinessProfile
                {
                    Id = await GenerateNextId(),
                    UserId = userId,
                    BusinessName = dto.BusinessName,
                    Industry = dto.Industry,
                    AnnualRevenue = dto.AnnualRevenue,
                    EmployeeCount = dto.EmployeeCount,
                    City = dto.City,
                    IsProfileCompleted = true // Auto-validates completing the flow here
                };
                await _profileRepository.AddAsync(profile);
            }
            else
            {
                // Update branch to persist late-bound information modifications securely
                profile.Industry = dto.Industry;
                profile.AnnualRevenue = dto.AnnualRevenue;
                profile.EmployeeCount = dto.EmployeeCount;
                profile.City = dto.City;
                profile.IsProfileCompleted = true;
                profile.UpdatedAt = DateTime.UtcNow;
                await _profileRepository.UpdateAsync(profile);
            }

            return MapToDto(profile);
        }

        // Explicit edit flow for a given ID ensuring that the targeted user truly owns the profile being modified.
        public async Task<BusinessProfileDto> UpdateProfileAsync(string userId, UpdateBusinessProfileDto dto)
        {
            var profile = await _profileRepository.GetByIdAsync(dto.Id); // Update by ID now
            if (profile == null || profile.UserId != userId)
                throw new KeyNotFoundException("Business profile not found.");

            // Directly overwrite the domain model attributes safely relying on EF entity tracking
            profile.BusinessName = dto.BusinessName;
            profile.Industry = dto.Industry;
            profile.AnnualRevenue = dto.AnnualRevenue;
            profile.EmployeeCount = dto.EmployeeCount;
            profile.City = dto.City;
            profile.IsProfileCompleted = dto.IsProfileCompleted;
            profile.UpdatedAt = DateTime.UtcNow;

            await _profileRepository.UpdateAsync(profile);
            return MapToDto(profile);
        }

        // Map function isolating the Data Transfer payload representing KYC data
        private static BusinessProfileDto MapToDto(BusinessProfile profile)
        {
            return new BusinessProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                BusinessName = profile.BusinessName,
                Industry = profile.Industry,
                AnnualRevenue = profile.AnnualRevenue,
                EmployeeCount = profile.EmployeeCount,
                City = profile.City,
                IsProfileCompleted = profile.IsProfileCompleted,
                CreatedAt = profile.CreatedAt
            };
        }
    }
}
