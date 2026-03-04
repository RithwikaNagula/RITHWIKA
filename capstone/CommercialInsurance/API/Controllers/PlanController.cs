// Provides core functionality and structures for the application.
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlanController : ControllerBase
    {
        private readonly IPlanService _planService;

        public PlanController(IPlanService planService)
        {
            _planService = planService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePlanDto dto)
        {
            var result = await _planService.CreatePlanAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _planService.GetAllPlansAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _planService.GetPlanByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("by-type/{insuranceTypeId}")]
        public async Task<IActionResult> GetByInsuranceType(string insuranceTypeId)
        {
            var result = await _planService.GetPlansByInsuranceTypeAsync(insuranceTypeId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _planService.DeletePlanAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
