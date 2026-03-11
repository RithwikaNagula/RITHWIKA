// Provides system-wide analytics for the admin panel: policy trends, claim settlements, premium revenue, and user growth metrics.
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

        // Inject IAnalyticsService which queries the database for pre-aggregated analytics data
        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        // GET api/analytics/agent-performance
        // Returns per-agent KPIs: policies issued, total premium, commission earned, and conversion rate.
        // Used to populate the admin analytics dashboard's agent comparison table.
        [HttpGet("agent-performance")]
        public async Task<ActionResult<List<AgentPerformanceDto>>> GetAgentPerformance()
        {
            var result = await _analyticsService.GetAgentPerformanceAsync();
            return Ok(result);
        }

        // GET api/analytics/claims-performance
        // Returns claim-volume breakdown: submitted today, pending, approved, rejected, and avg resolution time.
        [HttpGet("claims-performance")]
        public async Task<ActionResult<ClaimsAnalyticsDto>> GetClaimsPerformance()
        {
            var result = await _analyticsService.GetClaimsPerformanceAsync();
            return Ok(result);
        }

        // GET api/analytics/revenue?period=monthly|yearly
        // Returns time-series revenue data for the selected period; used by the admin revenue chart.
        // Defaults to "monthly" if no period query param is supplied.
        [HttpGet("revenue")]
        public async Task<ActionResult<RevenueAnalyticsDto>> GetRevenueAnalytics([FromQuery] string period = "monthly")
        {
            var result = await _analyticsService.GetRevenueAnalyticsAsync(period);
            return Ok(result);
        }
    }
}
