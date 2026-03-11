// Accepts support/contact form submissions from customers and lists all inquiries for admin review.
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

        // ISupportInquiryService records the contact form message and alerts admins via SignalR
        public SupportInquiryController(ISupportInquiryService inquiryService)
        {
            _inquiryService = inquiryService;
        }

        // POST api/supportinquiry
        // Public endpoint: accepts contact inquiries from both authenticated users and anonymous site visitors.
        // It saves the message and dispatches a notification to all Admin user accounts.
        [HttpPost]
        public async Task<ActionResult<SupportInquiryDto>> SubmitInquiry(CreateSupportInquiryDto dto)
        {
            var result = await _inquiryService.CreateInquiryAsync(dto);
            return Ok(result);
        }

        // GET api/supportinquiry
        // Admin-only: retrieves a chronologically ordered list of all contact inquiries.
        // Provides the data for the admin Helpdesk/Support dashboard component.
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<SupportInquiryDto>>> GetAll()
        {
            var result = await _inquiryService.GetAllInquiriesAsync();
            return Ok(result);
        }

        // PUT api/supportinquiry/{id}/resolve
        // Admin-only: toggles the inquiry's status to IsResolved = true.
        // It provides no content (204) back natively, indicating success.
        [HttpPut("{id}/resolve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Resolve(string id)
        {
            await _inquiryService.MarkAsResolvedAsync(id);
            return NoContent();
        }
    }
}
