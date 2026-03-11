// CRUD endpoints for business profiles linked to customer accounts; a profile is required before purchasing a policy.
using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/customer/business-profile")]
    [Authorize(Roles = "Customer")]
    public class BusinessProfileController : ControllerBase
    {
        private readonly IBusinessProfileService _profileService;

        // IBusinessProfileService handles all profile storage and validation logic
        public BusinessProfileController(IBusinessProfileService profileService)
        {
            _profileService = profileService;
        }

        // GET api/customer/business-profile
        // Returns all business profiles belonging to the logged-in customer.
        // A customer can have multiple profiles (one per business entity).
        [HttpGet]
        public async Task<IActionResult> GetProfiles()
        {
            // Extract the caller's user ID from the JWT NameIdentifier claim (set during login)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _profileService.GetProfilesByUserIdAsync(userId);
            return Ok(result);
        }

        // GET api/customer/business-profile/{id}
        // Returns a single profile by its primary key; returns 404 if not found or belongs to another user.
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var result = await _profileService.GetByIdAsync(id);
            if (result == null) return NotFound("Business profile not found.");
            return Ok(result);
        }

        // POST api/customer/business-profile
        // Creates or updates a profile. Uses Upsert logic: if a profile with the same BusinessName
        // already exists for this customer it is updated instead of creating a duplicate.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBusinessProfileDto dto)
        {
            // Extract the caller's user ID from the JWT NameIdentifier claim (set during login)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _profileService.UpsertProfileAsync(userId, dto);
            return Ok(result);
        }

        // PUT api/customer/business-profile
        // Updates an existing profile by ID. Ownership is verified (profile.UserId == caller ID).
        // Returns 404 if the profile does not exist or the caller does not own it.
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateBusinessProfileDto dto)
        {
            // Extract the caller's user ID from the JWT NameIdentifier claim (set during login)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            try
            {
                var result = await _profileService.UpdateProfileAsync(userId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
