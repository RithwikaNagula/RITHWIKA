// Handles policy premium payments: initiate payment, confirm payment success, and retrieve payment history for a policy.
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All payment endpoints require standard user authentication
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        // IPaymentService handles transaction recording, status updates, and retrieving payment records
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // POST api/payment
        // Customer endpoint: submits a new premium payment.
        // Accepts the payment method details and the associated policy ID.
        // Calculates the amount based on the policy and marks it paid on success.
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RecordPayment([FromBody] CreatePaymentDto dto)
        {
            // Override PaidByUserId directly from the JWT to prevent a user from claiming they are someone else
            dto.PaidByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _paymentService.ProcessPaymentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // GET api/payment/{id}
        // Retrieves the details of a specific payment transaction securely.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // GET api/payment/{id}/invoice
        // Returns an invoice summary object for the specified payment transaction,
        // which the frontend uses to render an HTML invoice receipt or generate a PDF.
        [HttpGet("{id}/invoice")]
        public async Task<IActionResult> GetInvoice(string id)
        {
            var result = await _paymentService.GetInvoiceAsync(id);
            return Ok(result);
        }

        // GET api/payment/policy/{policyId}
        // Retrieves payment history (only successful payments) for a given policy.
        [HttpGet("policy/{policyId}")]
        public async Task<IActionResult> GetByPolicy(string policyId)
        {
            var result = await _paymentService.GetPaymentsByPolicyAsync(policyId);
            return Ok(result);
        }

        // GET api/payment/policy/{policyId}/all
        // Retrieves all payment attempts (including failed/declined) for a given policy, useful for audits.
        [HttpGet("policy/{policyId}/all")]
        public async Task<IActionResult> GetAllByPolicy(string policyId)
        {
            var result = await _paymentService.GetAllPaymentsByPolicyAsync(policyId);
            return Ok(result);
        }

        // GET api/payment/policy/{policyId}/schedule
        // Retrieves upcoming / future scheduled payments if the policy is configured for monthly installments.
        [HttpGet("policy/{policyId}/schedule")]
        public async Task<IActionResult> GetSchedule(string policyId)
        {
            var result = await _paymentService.GetPaymentScheduleAsync(policyId);
            return Ok(result);
        }
    }
}
