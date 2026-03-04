using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupportInquiryController : ControllerBase
    {
        private readonly ISupportInquiryService _inquiryService;

        public SupportInquiryController(ISupportInquiryService inquiryService)
        {
            _inquiryService = inquiryService;
        }

        [HttpPost]
        public async Task<ActionResult<SupportInquiryDto>> SubmitInquiry(CreateSupportInquiryDto dto)
        {
            var result = await _inquiryService.CreateInquiryAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<SupportInquiryDto>>> GetAll()
        {
            var result = await _inquiryService.GetAllInquiriesAsync();
            return Ok(result);
        }

        [HttpPut("{id}/resolve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Resolve(string id)
        {
            await _inquiryService.MarkAsResolvedAsync(id);
            return NoContent();
        }
    }
}
