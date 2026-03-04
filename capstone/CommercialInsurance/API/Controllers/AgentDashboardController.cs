// Provides core functionality and structures for the application.
using System.Security.Claims;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Agent")]
    public class AgentDashboardController : ControllerBase
    {
        private readonly IAgentDashboardService _agentDashboardService;

        public AgentDashboardController(IAgentDashboardService agentDashboardService)
        {
            _agentDashboardService = agentDashboardService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var stats = await _agentDashboardService.GetAgentDashboardStatsAsync(agentId);
            return Ok(stats);
        }
    }
}
