// Service contract for authentication: register, login, password reset, and profile updates.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IAuthService
    {
        // Validates credentials and returns a signed JWT with the user's profile
        Task<LoginResponseDto> LoginAsync(LoginDto dto);
        // Creates a new Customer account with auto-assigned agent and claims officer
        Task<UserDto> RegisterAsync(RegisterDto dto);
        // Returns all users matching the given role string (Admin, Agent, ClaimsOfficer, Customer)
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role);
        // Admin-only: creates a user with an explicitly specified role
        Task<UserDto> CreateUserWithRoleAsync(RegisterDto dto, string role);
        // Looks up a single user by primary key; returns null if not found
        Task<UserDto?> GetUserByIdAsync(string userId);
        // Permanently deletes a user and reassigns their customers/policies/claims
        Task<bool> DeleteUserAsync(string userId);
        // Admin updates an Agent or ClaimsOfficer's profile (name, email, optional password)
        Task<UserDto> UpdateUserAsync(string userId, RegisterDto dto);
        // Allows any authenticated user to update their own name and email
        Task<UserDto> UpdateProfileAsync(string userId, UpdateProfileDto dto);
        // Sends a password reset link to the given email address
        Task ForgotPasswordAsync(string email);
        // Replaces the user's password hash with a new one derived from the supplied password
        Task ResetPasswordAsync(ResetPasswordDto dto);
    }
}
