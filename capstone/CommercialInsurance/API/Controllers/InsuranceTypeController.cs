// Provides core functionality and structures for the application.
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class InsuranceTypeController : ControllerBase
    {
        private readonly IInsuranceTypeService _service;

        public InsuranceTypeController(IInsuranceTypeService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInsuranceTypeDto dto)
        {
            var result = await _service.CreateInsuranceTypeAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllInsuranceTypesAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetInsuranceTypeByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _service.DeleteInsuranceTypeAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
