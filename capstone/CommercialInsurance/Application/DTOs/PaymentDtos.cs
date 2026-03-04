using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreatePaymentDto
    {
        [Required]
        public string PolicyId { get; set; } = string.Empty;

        [Required]
        public string PaymentFrequency { get; set; } = string.Empty; // Monthly | Quarterly | HalfYearly | Annually

        [Required]
        public string PaymentMode { get; set; } = string.Empty; // UPI | NetBanking | CreditCard | DebitCard

        public string? PaidByUserId { get; set; }
    }

    public class PaymentResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string PolicyId { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal AnnualPremium { get; set; }
        public decimal TotalPremium { get; set; }
        public string PaymentFrequency { get; set; } = string.Empty;
        public string PaymentMode { get; set; } = string.Empty;
        public int InstallmentNumber { get; set; }
        public int TotalInstallments { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string InsuranceTypeName { get; set; } = string.Empty;
        public decimal SelectedCoverageAmount { get; set; }
    }

    public class PaymentDto
    {
        public string Id { get; set; } = string.Empty;
        public string PolicyId { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string InvoiceInfo { get; set; } = string.Empty;
    }

    public class InvoiceDto
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string InsuranceTypeName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public decimal SelectedCoverageAmount { get; set; }
        public decimal AnnualPremium { get; set; }
        public decimal InstallmentAmount { get; set; }
        public int InstallmentNumber { get; set; }
        public int TotalInstallments { get; set; }
        public string PaymentFrequency { get; set; } = string.Empty;
        public string PaymentMode { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public DateTime DueDate { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class PaymentScheduleDto
    {
        public string PolicyId { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public decimal AnnualPremium { get; set; }
        public decimal InstallmentAmount { get; set; }
        public string PaymentFrequency { get; set; } = string.Empty;
        public int TotalInstallments { get; set; }
        public List<InstallmentDto> Schedule { get; set; } = new();
    }

    public class InstallmentDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public int InstallmentNumber { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
    }
}
