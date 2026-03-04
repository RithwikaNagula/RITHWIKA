// this controller manages every operation related to insurance claims and connects the user interface to the claim logic
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
    public class ClaimController : ControllerBase
    {
        // this is the service that contains all the actual business logic for claims
        private readonly IClaimService _claimService;

        public ClaimController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        [HttpPost("{policyId}")]
        [Authorize(Roles = "Customer")]
        // this action allows customers to file a new claim for an existing policy
        public async Task<IActionResult> FileClaim(string policyId, [FromForm] CreateClaimDto dto, IEnumerable<IFormFile> files)
        {
            var result = await _claimService.FileClaimAsync(dto, policyId, files);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "ClaimsOfficer")]
        // this action lets a claims officer change the status of a specific claim
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateClaimStatusDto dto)
        {
            var officerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _claimService.UpdateClaimStatusAsync(id, dto, officerId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        // this action retrieves the full details of a single claim record
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _claimService.GetClaimByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("by-policy/{policyId}")]
        public async Task<IActionResult> GetByPolicy(string policyId)
        {
            var result = await _claimService.GetClaimsByPolicyAsync(policyId);
            return Ok(result);
        }

        [HttpGet("my-assignments")]
        [Authorize(Roles = "ClaimsOfficer")]
        public async Task<IActionResult> GetMyAssignments()
        {
            var officerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _claimService.GetClaimsByOfficerAsync(officerId);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,ClaimsOfficer")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _claimService.GetAllClaimsAsync();
            return Ok(result);
        }
    }
}
