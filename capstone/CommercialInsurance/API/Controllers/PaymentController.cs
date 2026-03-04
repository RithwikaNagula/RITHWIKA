// Provides core functionality and structures for the application.
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RecordPayment([FromBody] CreatePaymentDto dto)
        {
            // Override PaidByUserId from JWT for security
            dto.PaidByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _paymentService.ProcessPaymentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("{id}/invoice")]
        public async Task<IActionResult> GetInvoice(string id)
        {
            var result = await _paymentService.GetInvoiceAsync(id);
            return Ok(result);
        }

        [HttpGet("policy/{policyId}")]
        public async Task<IActionResult> GetByPolicy(string policyId)
        {
            var result = await _paymentService.GetPaymentsByPolicyAsync(policyId);
            return Ok(result);
        }

        [HttpGet("policy/{policyId}/all")]
        public async Task<IActionResult> GetAllByPolicy(string policyId)
        {
            var result = await _paymentService.GetAllPaymentsByPolicyAsync(policyId);
            return Ok(result);
        }

        [HttpGet("policy/{policyId}/schedule")]
        public async Task<IActionResult> GetSchedule(string policyId)
        {
            var result = await _paymentService.GetPaymentScheduleAsync(policyId);
            return Ok(result);
        }
    }
}
