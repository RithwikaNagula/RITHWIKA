// Full policy management: request a quote, approve/issue a policy (agent), auto-renew, retrieve policies by user or agent, and upload policy documents.
using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Every flow in the policy lifecycle requires an authenticated customer, agent, or admin
    public class PolicyController : ControllerBase
    {
        // IPolicyService is the core engine managing quotes, approval boundaries, renewals, and deletions
        private readonly IPolicyService _policyService;

        public PolicyController(IPolicyService policyService)
        {
            _policyService = policyService;
        }

        // POST api/policy/request-purchase
        // Customer endpoint: submits a new policy quote request.
        // Requires a completed business profile, the selected plan, coverage amount, and evidence documents.
        // Creates a PendingReview policy and auto-assigns it to the agent with the lowest workload.
        [HttpPost("request-purchase")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RequestPurchase([FromForm] CreatePolicyDto dto, IEnumerable<IFormFile> documents)
        {
            // Extract the customer ID securely from the JWT; never trust external input for ownership
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _policyService.CreatePolicyAsync(dto, userId, documents);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // POST api/policy/agent-create
        // Agent endpoint: an agent creating a policy on behalf of a customer (assisted mode).
        // Functions exactly like request-purchase but explicitly sets the creator and assignee to the calling agent.
        [HttpPost("agent-create")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> AgentCreate([FromForm] CreatePolicyDto dto, IEnumerable<IFormFile> documents)
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _policyService.CreatePolicyAsync(dto, agentId, documents, agentId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // POST api/policy/{id}/approve
        // Agent endpoint: approves a PendingReview quote.
        // Triggers a notification to the customer allowing them to proceed to payment.
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> ApprovePolicy(string id)
        {
            var result = await _policyService.ApprovePolicyAsync(id);
            return Ok(result);
        }

        // POST api/policy/{id}/reject
        // Agent endpoint: rejects a policy quote.
        // Requires a reason (in RejectPolicyDto) which is logged and sent to the customer via notification.
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> RejectPolicy(string id, [FromBody] RejectPolicyDto dto)
        {
            var result = await _policyService.RejectPolicyAsync(id, dto);
            return Ok(result);
        }

        // GET api/policy/my-policies
        // Dynamic endpoint: returns a filtered list of policies based on role.
        // For agents: returns all policies they are assigned to manage.
        // For customers: returns only the policies they own.
        [HttpGet("my-policies")]
        public async Task<IActionResult> GetMyPolicies()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var role = User.FindFirstValue(ClaimTypes.Role);

            var result = role == "Agent"
                ? await _policyService.GetPoliciesByAgentAsync(userId)
                : await _policyService.GetPoliciesByUserAsync(userId);

            return Ok(result);
        }

        // GET api/policy/{id}
        // Retrieves the complete details of a single policy (plan info, quote amounts, status, uploaded docs).
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _policyService.GetPolicyByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // GET api/policy
        // Admin-only: enumerates every policy in the database for the global admin dashboard list.
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _policyService.GetAllPoliciesAsync();
            return Ok(result);
        }

        // POST api/policy/calculate-premium
        // Customer endpoint: calculates the quote based on the selected coverage and their business profile.
        // Factors in industry risk, employee count, and annual revenue multipliers.
        [HttpPost("calculate-premium")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CalculatePremium([FromBody] PremiumCalculationRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            request.CustomerId = userId;
            var result = await _policyService.CalculatePremiumAsync(request);
            return Ok(result);
        }

        // POST api/policy/agent/calculate-premium
        // Agent endpoint: runs the premium quote calculator on behalf of a customer.
        [HttpPost("agent/calculate-premium")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> AgentCalculatePremium([FromBody] PremiumCalculationRequestDto request)
        {
            var result = await _policyService.CalculatePremiumAsync(request);
            return Ok(result);
        }

        // POST api/policy/{id}/activate
        // Agent endpoint: forcefully activates an approved policy manually (bypassing normal payment auto-activation).
        // Useful for bank-transfer setups or manual underwriting overrides.
        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> ActivatePolicy(string id)
        {
            var result = await _policyService.IssuePolicyAsync(id);
            return Ok(result);
        }

        // POST api/policy/{id}/toggle-autorenew
        // Customer endpoint: toggles the auto-renew flag on an active policy.
        [HttpPost("{id}/toggle-autorenew")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ToggleAutoRenew(string id)
        {
            var result = await _policyService.ToggleAutoRenewAsync(id);
            return Ok(result);
        }

        // POST api/policy/{id}/renew
        // Customer endpoint: triggers manual renewal of an active (nearing expiry) or expired policy.
        // Re-evaluates premium limits and generates a new active policy term while superseding the old one.
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
                // Returns 400 Bad Request if renewal business rules fail (e.g., trying to renew > 30 days early)
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET api/policy/all
        // Admin-only: alternate alias endpoint for fetching all system policies.
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPolicies()
        {
            var result = await _policyService.GetAllPoliciesAsync();
            return Ok(result);
        }

        // DELETE api/policy/{id}
        // Admin-only: permanently cascade-deletes a policy.
        // Automatically strips all child payments, claims, history logs, and physical document files on disk.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePolicy(string id)
        {
            var result = await _policyService.DeletePolicyAsync(id);
            if (!result) return NotFound(new { message = "Policy not found." });
            return Ok(new { message = "Policy deleted successfully" });
        }
    }
}
