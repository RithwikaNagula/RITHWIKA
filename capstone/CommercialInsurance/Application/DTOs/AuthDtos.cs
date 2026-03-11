// Data Transfer Objects for authentication flows: RegisterDto, LoginDto, ForgotPasswordDto, ResetPasswordDto, UpdateProfileDto, and AuthResponseDto.
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    // Inbound payload for new user registration; validated with DataAnnotations before reaching the service layer
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.(com|in|org|net|co|edu|gov|io)$", ErrorMessage = "Email must have a valid extension like .com, .in, etc.")]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }

    // Inbound payload for user authentication; email format is validated with a regex whitelist
    public class LoginDto
    {
        [Required, EmailAddress]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.(com|in|org|net|co|edu|gov|io)$", ErrorMessage = "Email must have a valid extension like .com, .in, etc.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    // Outbound payload returned after successful login containing the signed JWT and the user's profile
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
    }

    // Safe outbound representation of a user (no password hash); includes assigned agent/officer names
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string? AssignedAgentId { get; set; }
        public string? AssignedAgentName { get; set; }
        public string? AssignedClaimsOfficerId { get; set; }
        public string? AssignedClaimsOfficerName { get; set; }
    }
    // Inbound payload for initiating a password reset; only the email is required
    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    // Inbound payload for completing a password reset with a new password
    public class ResetPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    // Inbound payload allowing an authenticated user to update their own name and email
    public class UpdateProfileDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
