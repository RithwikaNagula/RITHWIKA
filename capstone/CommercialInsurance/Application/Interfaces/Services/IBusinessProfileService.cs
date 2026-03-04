// Provides core functionality and structures for the application.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IBusinessProfileService
    {
        Task<BusinessProfileDto?> GetByIdAsync(string id);
        Task<IEnumerable<BusinessProfileDto>> GetProfilesByUserIdAsync(string userId);
        Task<BusinessProfileDto?> GetByUserIdAsync(string userId);
        Task<BusinessProfileDto> UpsertProfileAsync(string userId, CreateBusinessProfileDto dto);
        Task<BusinessProfileDto> UpdateProfileAsync(string userId, UpdateBusinessProfileDto dto);
    }
}
