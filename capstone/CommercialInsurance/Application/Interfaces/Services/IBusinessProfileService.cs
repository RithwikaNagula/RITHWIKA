// Service contract for business profile CRUD operations tied to a customer.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IBusinessProfileService
    {
        // Returns a single profile by its ID; used when viewing or editing a specific profile
        Task<BusinessProfileDto?> GetByIdAsync(string id);
        // Returns all profiles belonging to a customer (a customer can have multiple business entities)
        Task<IEnumerable<BusinessProfileDto>> GetProfilesByUserIdAsync(string userId);
        // Returns the first profile for a user; used as a quick check during quote generation
        Task<BusinessProfileDto?> GetByUserIdAsync(string userId);
        // Creates a new profile or updates an existing one with the same business name (upsert)
        Task<BusinessProfileDto> UpsertProfileAsync(string userId, CreateBusinessProfileDto dto);
        // Updates a profile by ID after verifying the caller owns it
        Task<BusinessProfileDto> UpdateProfileAsync(string userId, UpdateBusinessProfileDto dto);
    }
}
