// Manages the full claims lifecycle: file a new claim, list claims by policy or user, upload supporting documents, and update claim status (claims officer only).
using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All claim endpoints require an authenticated user
    public class ClaimController : ControllerBase
    {
        private readonly IClaimService _claimService;

        // IClaimService handles the full claim lifecycle: filing, status updates, document uploads
        public ClaimController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        // POST api/claim/{policyId}/file
        // Customer endpoint: submits a new claim against an active policy.
        // Accepts a multipart form: claim details (description, amount) + evidence file uploads.
        // The backend validates remaining coverage and assigns the correct claims officer automatically.
        [HttpPost("{policyId}/file")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> FileClaim(string policyId, [FromForm] CreateClaimDto dto, IEnumerable<IFormFile> files)
        {
            var result = await _claimService.FileClaimAsync(dto, policyId, files);
            return Ok(result);
        }

        // GET api/claim/policy/{policyId}
        // Returns all claims filed against a specific policy with their full history logs.
        // Used on the policy detail view to show the customer the status of each claim.
        [HttpGet("policy/{policyId}")]
        public async Task<IActionResult> GetByPolicy(string policyId)
        {
            var result = await _claimService.GetClaimsByPolicyAsync(policyId);
            return Ok(result);
        }

        // GET api/claim/officer
        // Returns all claims assigned to the authenticated claims officer; used to populate the officer worklist.
        [HttpGet("officer")]
        [Authorize(Roles = "ClaimsOfficer")]
        public async Task<IActionResult> GetByOfficer()
        {
            // Extract the officer's user ID from their JWT to scope the query
            var officerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _claimService.GetClaimsByOfficerAsync(officerId);
            return Ok(result);
        }

        // GET api/claim/all
        // Admin-only: retrieves every claim in the system for the admin claims overview page.
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _claimService.GetAllClaimsAsync();
            return Ok(result);
        }

        // GET api/claim/{id}
        // Retrieves a single claim by its ID, including full audit history and attached documents.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _claimService.GetClaimByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // PUT api/claim/{claimId}/status
        // Claims officer endpoint: updates the claim's status (UnderReview, Approved, Rejected, Settled)
        // and records an audit log entry with the officer's remarks.
        [HttpPut("{claimId}/status")]
        [Authorize(Roles = "ClaimsOfficer,Admin")]
        public async Task<IActionResult> UpdateStatus(string claimId, [FromBody] UpdateClaimStatusDto dto)
        {
            // Pass the officer's ID from the JWT so the audit log records who made the change
            var officerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _claimService.UpdateClaimStatusAsync(claimId, dto, officerId);
            return Ok(result);
        }
    }
}
