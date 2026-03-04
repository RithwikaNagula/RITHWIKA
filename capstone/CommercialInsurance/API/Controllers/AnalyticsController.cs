using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("agent-performance")]
        public async Task<ActionResult<List<AgentPerformanceDto>>> GetAgentPerformance()
        {
            var result = await _analyticsService.GetAgentPerformanceAsync();
            return Ok(result);
        }

        [HttpGet("claims-performance")]
        public async Task<ActionResult<ClaimsAnalyticsDto>> GetClaimsPerformance()
        {
            var result = await _analyticsService.GetClaimsPerformanceAsync();
            return Ok(result);
        }

        [HttpGet("revenue")]
        public async Task<ActionResult<RevenueAnalyticsDto>> GetRevenueAnalytics([FromQuery] string period = "monthly")
        {
            var result = await _analyticsService.GetRevenueAnalyticsAsync(period);
            return Ok(result);
        }
    }
}
