// Admin-managed insurance category endpoints: create, read, update, and delete insurance types (e.g. Auto, General Liability).
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsuranceTypeController : ControllerBase
    {
        private readonly IInsuranceTypeService _insuranceTypeService;

        // IInsuranceTypeService handles CRUD and uniqueness validation for insurance categories
        public InsuranceTypeController(IInsuranceTypeService insuranceTypeService)
        {
            _insuranceTypeService = insuranceTypeService;
        }

        // GET api/insurancetype
        // Public endpoint: returns all insurance categories with their associated plans.
        // Used on the landing page and the plan selection flow (no auth required).
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _insuranceTypeService.GetAllInsuranceTypesAsync();
            return Ok(result);
        }

        // GET api/insurancetype/{id}
        // Returns a single insurance type with its nested plans; used on the plan detail page.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _insuranceTypeService.GetInsuranceTypeByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // POST api/insurancetype
        // Admin-only: creates a new insurance category.
        // The service enforces that the category name must be unique across the system.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateInsuranceTypeDto dto)
        {
            var result = await _insuranceTypeService.CreateInsuranceTypeAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // PUT api/insurancetype/{id}
        // Admin-only: updates an existing insurance category's name or description.
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateInsuranceTypeDto dto)
        {
            var result = await _insuranceTypeService.UpdateInsuranceTypeAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // DELETE api/insurancetype/{id}
        // Admin-only: removes an insurance category.
        // Should only be called if no active plans or policies reference this type.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _insuranceTypeService.DeleteInsuranceTypeAsync(id);
            if (!success) return NotFound();
            return NoContent(); // 204 No Content on successful delete
        }
    }
}
