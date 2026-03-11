// Handles user authentication endpoints: register, login, forgot-password, reset-password, and authenticated profile update.
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // IAuthService is injected via DI; it handles all user credential and session logic
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST api/auth/register
        // Registers a new Customer account. Automatically assigns the least-loaded agent and claims officer.
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = await _authService.RegisterAsync(dto);
            return CreatedAtAction(nameof(Register), user); // 201 Created with the new user's profile
        }

        // POST api/auth/login
        // Validates credentials; on success returns a signed JWT token and the user's profile DTO.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var response = await _authService.LoginAsync(dto);
            return Ok(response); // 200 OK with { token, user }
        }

        // POST api/auth/forgot-password
        // Accepts an email address and emails a password reset link.
        // Currently simulated via Console.WriteLine; no actual email is sent.
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _authService.ForgotPasswordAsync(dto.Email);
            return Ok(new { message = "Reset instructions sent." });
        }

        // POST api/auth/reset-password
        // Accepts an email and new password, re-hashes the password using BCrypt, and saves it.
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            await _authService.ResetPasswordAsync(dto);
            return Ok(new { message = "Password reset successfully." });
        }

        // GET api/auth/profile
        // Returns the safe DTO for the currently authenticated user.
        // Used by the frontend to refresh the local user object (e.g., after agent reassignments).
        [HttpGet("profile")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // PUT api/auth/update-profile
        // Allows an authenticated user to update their own FullName and Email.
        // Extracts the user ID from the JWT NameIdentifier claim instead of trusting the request body.
        [HttpPut("update-profile")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            // Read the user's ID from the JWT claims — prevents users from editing each other's profiles
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _authService.UpdateProfileAsync(userId, dto);
            return Ok(user);
        }
    }
}