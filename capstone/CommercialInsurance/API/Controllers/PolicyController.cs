// this controller handles all operations related to insurance policies including creation approval and renewal
using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PolicyController : ControllerBase
    {
        // this is the primary service used to manage the lifecycle of insurance policies
        private readonly IPolicyService _policyService;

        public PolicyController(IPolicyService policyService)
        {
            _policyService = policyService;
        }

        [HttpPost("request-purchase")]
        [Authorize(Roles = "Customer")]
        // this action allows a customer to request to buy a new insurance policy
        public async Task<IActionResult> RequestPurchase([FromForm] CreatePolicyDto dto, IEnumerable<IFormFile> documents)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            // In RequestPurchase, the customerId is set from the token for security
            var result = await _policyService.CreatePolicyAsync(dto, userId, documents);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("agent-create")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> AgentCreate([FromForm] CreatePolicyDto dto, IEnumerable<IFormFile> documents)
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            // Assisted mode: agent is the creator
            var result = await _policyService.CreatePolicyAsync(dto, agentId, documents, agentId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Agent")]
        // this action allows an insurance agent to approve a pending policy request
        public async Task<IActionResult> ApprovePolicy(string id)
        {
            var result = await _policyService.ApprovePolicyAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> RejectPolicy(string id, [FromBody] RejectPolicyDto dto)
        {
            var result = await _policyService.RejectPolicyAsync(id, dto);
            return Ok(result);
        }

        [HttpGet("my-policies")]
        // this action gets a list of policies belonging to the currently logged in user or agent
        public async Task<IActionResult> GetMyPolicies()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var role = User.FindFirstValue(ClaimTypes.Role);

            var result = role == "Agent"
                ? await _policyService.GetPoliciesByAgentAsync(userId)
                : await _policyService.GetPoliciesByUserAsync(userId);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _policyService.GetPolicyByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _policyService.GetAllPoliciesAsync();
            return Ok(result);
        }

        [HttpPost("calculate-premium")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CalculatePremium([FromBody] PremiumCalculationRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            request.CustomerId = userId;
            var result = await _policyService.CalculatePremiumAsync(request);
            return Ok(result);
        }

        [HttpPost("agent/calculate-premium")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> AgentCalculatePremium([FromBody] PremiumCalculationRequestDto request)
        {
            var result = await _policyService.CalculatePremiumAsync(request);
            return Ok(result);
        }

        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> ActivatePolicy(string id)
        {
            var result = await _policyService.IssuePolicyAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/toggle-autorenew")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ToggleAutoRenew(string id)
        {
            var result = await _policyService.ToggleAutoRenewAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/renew")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RenewPolicy(string id)
        {
            try
            {
                var result = await _policyService.RenewPolicyAsync(id);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPolicies()
        {
            var result = await _policyService.GetAllPoliciesAsync();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        // this action allows an administrator to completely remove a policy from the system
        public async Task<IActionResult> DeletePolicy(string id)
        {
            var result = await _policyService.DeletePolicyAsync(id);
            if (!result) return NotFound(new { message = "Policy not found." });
            return Ok(new { message = "Policy deleted successfully" });
        }
    }
}
