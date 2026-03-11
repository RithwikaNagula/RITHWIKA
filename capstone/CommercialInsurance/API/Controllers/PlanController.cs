// Exposes plan data per insurance type; used by customers during the quote and plan-selection flow.
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlanController : ControllerBase
    {
        private readonly IPlanService _planService;

        // IPlanService abstracts logic for retrieving base rates and coverage limits for each insurance plan
        public PlanController(IPlanService planService)
        {
            _planService = planService;
        }

        // POST api/plan
        // Admin-only: creates a new insurance plan (e.g. "Premium Cover") under an existing insurance type.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreatePlanDto dto, [FromForm] IFormFile? image)
        {
            if (image != null && image.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "plans");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                dto.ImageUrl = $"/uploads/plans/{fileName}";
            }

            var result = await _planService.CreatePlanAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // GET api/plan
        // Public endpoint: returns all plans across all insurance types.
        // Used on marketing or main coverage overview pages.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _planService.GetAllPlansAsync();
            return Ok(result);
        }

        // GET api/plan/{id}
        // Public endpoint: returns details of a single plan (including base premium and min/max coverage).
        // Essential for the frontend policy premium calculator.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _planService.GetPlanByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // GET api/plan/by-type/{insuranceTypeId}
        // Public endpoint: fetches all plan tiers (Basic, Standard, Premium) available under a specific category.
        // Used when a customer selects a category like 'General Liability' to show them available plans.
        [HttpGet("by-type/{insuranceTypeId}")]
        public async Task<IActionResult> GetByInsuranceType(string insuranceTypeId)
        {
            var result = await _planService.GetPlansByInsuranceTypeAsync(insuranceTypeId);
            return Ok(result);
        }

        // DELETE api/plan/{id}
        // Admin-only: permanently deletes an insurance plan.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _planService.DeletePlanAsync(id);
            if (!success) return NotFound();
            return NoContent(); // Code 204: Successfully deleted
        }
    }
}
