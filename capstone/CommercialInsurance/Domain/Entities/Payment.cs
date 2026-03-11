// Records a single premium payment transaction: amount, date, status, and reference to the policy.
using Domain.Enums;

namespace Domain.Entities
{
    public class Payment
    {
        public string Id { get; set; } = string.Empty;
        // Foreign key to the policy this payment applies to
        public string PolicyId { get; set; } = string.Empty;
        // Installment amount for this specific payment (annual premium / total installments)
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        // Tracks whether the payment was completed, is pending, or failed
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        // Unique system-generated reference ID (TXN-XXXXXXXX) for tracing this transaction
        public string TransactionId { get; set; } = string.Empty;

        // Installment-based payment fields
        // Auto-generated invoice number in the format INV-XXXXXXXX
        public string InvoiceNumber { get; set; } = string.Empty;
        // Payment cadence mirrored from the policy: Monthly, Quarterly, HalfYearly, or Annually
        public string PaymentFrequency { get; set; } = string.Empty;
        // Which installment this is in the sequence (1 of N)
        public int InstallmentNumber { get; set; } = 1;
        // Total number of installments based on the payment frequency and policy duration
        public int TotalInstallments { get; set; } = 1;
        // Scheduled due date for this installment
        public DateTime DueDate { get; set; }
        // Payment method used: UPI, NetBanking, CreditCard, or DebitCard
        public string PaymentMode { get; set; } = string.Empty;
        // Optional: the user who made the payment (useful when agents pay on behalf of customers)
        public string? PaidByUserId { get; set; }

        // Navigation properties
        public Policy Policy { get; set; } = null!;
        public User? PaidByUser { get; set; }
    }
}
