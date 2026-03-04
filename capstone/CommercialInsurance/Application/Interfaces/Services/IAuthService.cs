// Provides core functionality and structures for the application.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto dto);
        Task<UserDto> RegisterAsync(RegisterDto dto);
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role);
        Task<UserDto> CreateUserWithRoleAsync(RegisterDto dto, string role);
        Task<UserDto?> GetUserByIdAsync(string userId);
        Task<bool> DeleteUserAsync(string userId);
        Task<UserDto> UpdateUserAsync(string userId, RegisterDto dto);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDto dto);
    }
}
