// Exposes agent dashboard data: assigned policies, claims overview, and aggregate performance statistics for the logged-in agent.
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

        // IAgentDashboardService aggregtes data specifically for the currently logged-in agent
        public AgentDashboardController(IAgentDashboardService agentDashboardService)
        {
            _agentDashboardService = agentDashboardService;
        }

        // GET api/agentdashboard/stats
        // Returns the key performance indicators for the agent (e.g., active policies they manage,
        // pending policies requiring their approval, and their total commission earned).
        // Used to populate the summary cards at the top of the Agent Dashboard UI.
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            // The agent ID is extracted securely from the JWT claim to prevent data spoofing
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var stats = await _agentDashboardService.GetAgentDashboardStatsAsync(agentId);
            return Ok(stats);
        }
    }
}
