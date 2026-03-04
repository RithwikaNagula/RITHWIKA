// this controller manages administrative tasks such as viewing dashboard statistics and creating staff accounts
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminController(IAuthService authService, IAdminDashboardService adminDashboardService)
        {
            _authService = authService;
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet("dashboard-stats")]
        // this action fetches the primary statistics for the admin dashboard such as total users and earnings
        public async Task<IActionResult> GetDashboardStats()
        {
            var result = await _adminDashboardService.GetDashboardStatsAsync();
            return Ok(result);
        }

        [HttpGet("analytics-charts")]
        public async Task<IActionResult> GetAnalyticsCharts()
        {
            var result = await _adminDashboardService.GetAnalyticsAsync();
            return Ok(result);
        }

        [HttpPost("create-agent")]
        // this action creates a new agent user account in the system
        public async Task<IActionResult> CreateAgent([FromBody] RegisterDto dto)
        {
            var result = await _authService.CreateUserWithRoleAsync(dto, "Agent");
            return CreatedAtAction(nameof(CreateAgent), result);
        }

        [HttpPost("create-claims-officer")]
        public async Task<IActionResult> CreateClaimsOfficer([FromBody] RegisterDto dto)
        {
            var result = await _authService.CreateUserWithRoleAsync(dto, "ClaimsOfficer");
            return CreatedAtAction(nameof(CreateClaimsOfficer), result);
        }

        [HttpGet("agents")]
        public async Task<IActionResult> GetAgents()
        {
            var result = await _authService.GetUsersByRoleAsync("Agent");
            return Ok(result);
        }

        [HttpGet("claims-officers")]
        public async Task<IActionResult> GetClaimsOfficers()
        {
            var result = await _authService.GetUsersByRoleAsync("ClaimsOfficer");
            return Ok(result);
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers()
        {
            var result = await _authService.GetUsersByRoleAsync("Customer");
            return Ok(result);
        }

        [HttpPut("users/{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] RegisterDto dto)
        {
            try
            {
                var result = await _authService.UpdateUserAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("users/{userId}")]
        // this action permanently removes a user account from the database
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _authService.DeleteUserAsync(userId);
            if (!result) return NotFound();
            return Ok(new { message = "User deleted successfully" });
        }
    }
}
