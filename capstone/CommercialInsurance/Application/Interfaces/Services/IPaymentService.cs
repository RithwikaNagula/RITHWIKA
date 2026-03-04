// Provides core functionality and structures for the application.
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> ProcessPaymentAsync(CreatePaymentDto dto);
        Task<PaymentDto?> GetPaymentByIdAsync(string id);
        Task<IEnumerable<PaymentDto>> GetPaymentsByPolicyAsync(string policyId);
        Task<InvoiceDto> GetInvoiceAsync(string paymentId);
        Task<PaymentScheduleDto> GetPaymentScheduleAsync(string policyId);
        Task<List<PaymentResponseDto>> GetAllPaymentsByPolicyAsync(string policyId);
    }
}
