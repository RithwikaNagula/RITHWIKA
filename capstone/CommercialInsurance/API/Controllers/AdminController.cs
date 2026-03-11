// Admin-only endpoints for managing agents and claims officers: create accounts, list all users, and remove personnel.
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // All endpoints here are restricted to the Admin role
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAdminDashboardService _adminDashboardService;

        // Inject AuthService (for user management) and AdminDashboardService (for KPI aggregates)
        public AdminController(IAuthService authService, IAdminDashboardService adminDashboardService)
        {
            _authService = authService;
            _adminDashboardService = adminDashboardService;
        }

        // GET api/admin/dashboard-stats
        // Returns summary KPIs for the admin dashboard: total users per role, active policies, revenue, etc.
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var result = await _adminDashboardService.GetDashboardStatsAsync();
            return Ok(result);
        }

        // GET api/admin/analytics-charts
        // Returns data for time-series charts on the admin analytics page (monthly revenue, claim counts, etc.)
        [HttpGet("analytics-charts")]
        public async Task<IActionResult> GetAnalyticsCharts()
        {
            var result = await _adminDashboardService.GetAnalyticsAsync();
            return Ok(result);
        }

        // POST api/admin/create-agent
        // Creates a new Agent user account. The admin supplies name, email, and a temporary password.
        [HttpPost("create-agent")]
        public async Task<IActionResult> CreateAgent([FromBody] RegisterDto dto)
        {
            var result = await _authService.CreateUserWithRoleAsync(dto, "Agent");
            return CreatedAtAction(nameof(CreateAgent), result);
        }

        // POST api/admin/create-claims-officer
        // Creates a new ClaimsOfficer user account, identical flow to agent creation but with a different role.
        [HttpPost("create-claims-officer")]
        public async Task<IActionResult> CreateClaimsOfficer([FromBody] RegisterDto dto)
        {
            var result = await _authService.CreateUserWithRoleAsync(dto, "ClaimsOfficer");
            return CreatedAtAction(nameof(CreateClaimsOfficer), result);
        }

        // GET api/admin/agents
        // Returns a list of all users with the Agent role; used by the admin personnel directory.
        [HttpGet("agents")]
        public async Task<IActionResult> GetAgents()
        {
            var result = await _authService.GetUsersByRoleAsync("Agent");
            return Ok(result);
        }

        // GET api/admin/claims-officers
        // Returns a list of all ClaimsOfficer users; used by the admin personnel directory.
        [HttpGet("claims-officers")]
        public async Task<IActionResult> GetClaimsOfficers()
        {
            var result = await _authService.GetUsersByRoleAsync("ClaimsOfficer");
            return Ok(result);
        }

        // GET api/admin/customers
        // Returns all Customer accounts for admin visibility; not editable via this controller.
        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers()
        {
            var result = await _authService.GetUsersByRoleAsync("Customer");
            return Ok(result);
        }

        // PUT api/admin/users/{userId}
        // Updates an Agent or ClaimsOfficer account (name, email, optional password reset).
        // Note: Customer accounts cannot be edited by admin — enforced inside UpdateUserAsync.
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
                // Return 400 with the business rule message (e.g. "Customers cannot be edited by admin")
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE api/admin/users/{userId}
        // Permanently removes a user from the system.
        // Before deletion, any customers, policies, or claims assigned to the user are reassigned
        // to the least-loaded remaining agent/officer to prevent orphaned records.
        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _authService.DeleteUserAsync(userId);
            if (!result) return NotFound(); // User does not exist
            return Ok(new { message = "User deleted successfully" });
        }
    }
}
