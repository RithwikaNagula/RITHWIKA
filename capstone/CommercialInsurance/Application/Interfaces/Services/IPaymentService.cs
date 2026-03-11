// Service contract for processing premium payments and fetching payment history.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IPaymentService
    {
        // Records a payment transaction, calculates the installment amount, and updates policy payment status
        Task<PaymentResponseDto> ProcessPaymentAsync(CreatePaymentDto dto);
        // Retrieves a single payment by its primary key
        Task<PaymentDto?> GetPaymentByIdAsync(string id);
        // Returns all payments recorded for a specific policy (compact list view)
        Task<IEnumerable<PaymentDto>> GetPaymentsByPolicyAsync(string policyId);
        // Generates a printable invoice DTO for a completed payment
        Task<InvoiceDto> GetInvoiceAsync(string paymentId);
        // Returns the full installment schedule for a policy with individual due dates
        Task<PaymentScheduleDto> GetPaymentScheduleAsync(string policyId);
        // Returns all payments for a policy with full response details (invoice numbers, customer info)
        Task<List<PaymentResponseDto>> GetAllPaymentsByPolicyAsync(string policyId);
    }
}
