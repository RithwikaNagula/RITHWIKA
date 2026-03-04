// Provides core functionality and structures for the application.
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class BusinessProfileService : IBusinessProfileService
    {
        private readonly IBusinessProfileRepository _profileRepository;

        public BusinessProfileService(IBusinessProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<BusinessProfileDto?> GetByUserIdAsync(string userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null) return null;

            return MapToDto(profile);
        }

        public async Task<BusinessProfileDto?> GetByIdAsync(string id)
        {
            var profile = await _profileRepository.GetByIdAsync(id);
            if (profile == null) return null;

            return MapToDto(profile);
        }

        public async Task<IEnumerable<BusinessProfileDto>> GetProfilesByUserIdAsync(string userId)
        {
            var profiles = await _profileRepository.GetProfilesByUserIdAsync(userId);
            return profiles.Select(MapToDto);
        }

        private async Task<string> GenerateNextId()
        {
            var all = await _profileRepository.GetAllAsync();
            var count = all.Count();
            return $"bpro{count + 1}";
        }

        public async Task<BusinessProfileDto> UpsertProfileAsync(string userId, CreateBusinessProfileDto dto)
        {
            // Support multiple profiles: check if a profile with SAME BusinessName exists for this user
            var profiles = await _profileRepository.GetProfilesByUserIdAsync(userId);
            var profile = profiles.FirstOrDefault(p => p.BusinessName.Equals(dto.BusinessName, StringComparison.OrdinalIgnoreCase));

            if (profile == null)
            {
                profile = new BusinessProfile
                {
                    Id = await GenerateNextId(),
                    UserId = userId,
                    BusinessName = dto.BusinessName,
                    Industry = dto.Industry,
                    AnnualRevenue = dto.AnnualRevenue,
                    EmployeeCount = dto.EmployeeCount,
                    City = dto.City,
                    IsProfileCompleted = true
                };
                await _profileRepository.AddAsync(profile);
            }
            else
            {
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

        public async Task<BusinessProfileDto> UpdateProfileAsync(string userId, UpdateBusinessProfileDto dto)
        {
            var profile = await _profileRepository.GetByIdAsync(dto.Id); // Update by ID now
            if (profile == null || profile.UserId != userId)
                throw new KeyNotFoundException("Business profile not found.");

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
