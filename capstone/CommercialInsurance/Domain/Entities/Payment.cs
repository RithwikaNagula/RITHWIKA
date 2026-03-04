// Provides core functionality and structures for the application.
using Domain.Enums;

namespace Domain.Entities
{
    public class Payment
    {
        public string Id { get; set; } = string.Empty;
        public string PolicyId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string TransactionId { get; set; } = string.Empty;

        // New fields for installment-based payments
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PaymentFrequency { get; set; } = string.Empty;
        public int InstallmentNumber { get; set; } = 1;
        public int TotalInstallments { get; set; } = 1;
        public DateTime DueDate { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string? PaidByUserId { get; set; }

        // Navigation properties
        public Policy Policy { get; set; } = null!;
        public User? PaidByUser { get; set; }
    }
}
