// Provides core functionality and structures for the application.
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPlanRepository _planRepository;
        private readonly IBusinessProfileRepository _profileRepository;
        private readonly INotificationService _notificationService;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IPolicyRepository policyRepository,
            IUserRepository userRepository,
            IPlanRepository planRepository,
            IBusinessProfileRepository profileRepository,
            INotificationService notificationService)
        {
            _paymentRepository = paymentRepository;
            _policyRepository = policyRepository;
            _userRepository = userRepository;
            _planRepository = planRepository;
            _profileRepository = profileRepository;
            _notificationService = notificationService;
        }

        private async Task<string> GenerateNextId()
        {
            return Guid.NewGuid().ToString();
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            return $"INV-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString()[..5].ToUpper()}";
        }

        public async Task<PaymentResponseDto> ProcessPaymentAsync(CreatePaymentDto dto)
        {
            var policy = await _policyRepository.GetByIdAsync(dto.PolicyId);
            if (policy == null) throw new KeyNotFoundException("Policy not found.");

            if (policy.Status != PolicyStatus.Approved && policy.Status != PolicyStatus.PendingPayment)
                throw new InvalidOperationException("Only Approved or PendingPayment policies can be paid for.");

            var plan = await _planRepository.GetByIdAsync(policy.PlanId);
            var user = await _userRepository.GetByIdAsync(policy.UserId);
            var profile = !string.IsNullOrEmpty(policy.BusinessProfileId)
                ? await _profileRepository.GetByIdAsync(policy.BusinessProfileId)
                : null;

            // Premium Calculation: Annual Premium = (Coverage / 100) * 5
            decimal annualPremium = (policy.SelectedCoverageAmount / 100m) * 5m;

            int totalInstallments;
            decimal installmentAmount;
            int monthInterval;

            switch (dto.PaymentFrequency)
            {
                case "Monthly":
                    totalInstallments = 12;
                    installmentAmount = Math.Round(annualPremium / 12m, 2);
                    monthInterval = 1;
                    break;
                case "Quarterly":
                    totalInstallments = 4;
                    installmentAmount = Math.Round(annualPremium / 4m, 2);
                    monthInterval = 3;
                    break;
                case "HalfYearly":
                    totalInstallments = 2;
                    installmentAmount = Math.Round(annualPremium / 2m, 2);
                    monthInterval = 6;
                    break;
                case "Annually":
                    totalInstallments = 1;
                    installmentAmount = annualPremium;
                    monthInterval = 12;
                    break;
                default:
                    throw new ArgumentException($"Invalid PaymentFrequency: {dto.PaymentFrequency}");
            }

            var today = DateTime.UtcNow;
            Payment firstPayment = null!;

            // Create ALL installment payment records
            for (int i = 1; i <= totalInstallments; i++)
            {
                var dueDate = today.AddMonths((i - 1) * monthInterval);
                var invoiceNumber = await GenerateInvoiceNumberAsync();
                var paymentId = await GenerateNextId();

                var payment = new Payment
                {
                    Id = paymentId,
                    PolicyId = dto.PolicyId,
                    Amount = installmentAmount,
                    TransactionId = i == 1 ? $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}" : string.Empty,
                    PaymentDate = i == 1 ? today : DateTime.UtcNow,
                    Status = i == 1 ? PaymentStatus.Completed : PaymentStatus.Pending,
                    InvoiceNumber = invoiceNumber,
                    PaymentFrequency = dto.PaymentFrequency,
                    InstallmentNumber = i,
                    TotalInstallments = totalInstallments,
                    DueDate = dueDate,
                    PaymentMode = dto.PaymentMode,
                    PaidByUserId = dto.PaidByUserId
                };

                await _paymentRepository.AddAsync(payment);

                if (i == 1) firstPayment = payment;
            }

            // Activate the policy
            policy.Status = PolicyStatus.Active;
            policy.PaymentFrequency = dto.PaymentFrequency;

            // Only set dates if they are default/unset OR if it is a renewal
            if (policy.StartDate == default || policy.StartDate == DateTime.MinValue || policy.IsRenewal)
            {
                if (policy.StartDate == default || policy.StartDate == DateTime.MinValue)
                    policy.StartDate = today;
            }

            if (policy.EndDate == default || policy.EndDate == DateTime.MinValue || policy.IsRenewal)
            {
                if (policy.EndDate == default || policy.EndDate == DateTime.MinValue)
                    policy.EndDate = today.AddMonths(12);
            }

            // Update policy number if it's still a quote/renewal number
            if (policy.PolicyNumber.StartsWith("PRQ-") || policy.PolicyNumber.StartsWith("QTE-") || policy.PolicyNumber.StartsWith("RNW-"))
            {
                policy.PolicyNumber = "POL-" + Guid.NewGuid().ToString()[..8].ToUpper();
            }

            policy.CommissionStatus = CommissionStatus.Earned;
            await _policyRepository.UpdateAsync(policy);

            await _notificationService.CreateNotificationAsync(
                policy.UserId,
                "Payment Successful",
                $"Payment of {installmentAmount:C} for policy {policy.PolicyNumber} has been successfully processed.",
                NotificationType.Payment.ToString(),
                policy.Id
            );

            if (!string.IsNullOrEmpty(policy.AgentId))
            {
                await _notificationService.CreateNotificationAsync(
                    policy.AgentId,
                    "Client Payment Received",
                    $"Client {user?.FullName} has paid {installmentAmount:C} for policy {policy.PolicyNumber}.",
                    NotificationType.Payment.ToString(),
                    policy.Id
                );
            }

            return new PaymentResponseDto
            {
                Id = firstPayment.Id,
                PolicyId = policy.Id,
                PolicyNumber = policy.PolicyNumber,
                InvoiceNumber = firstPayment.InvoiceNumber,
                Amount = installmentAmount,
                AnnualPremium = annualPremium,
                TotalPremium = annualPremium,
                PaymentFrequency = dto.PaymentFrequency,
                PaymentMode = dto.PaymentMode,
                InstallmentNumber = 1,
                TotalInstallments = totalInstallments,
                DueDate = firstPayment.DueDate,
                PaymentDate = firstPayment.PaymentDate,
                Status = "Completed",
                TransactionId = firstPayment.TransactionId,
                CustomerName = user?.FullName ?? "",
                BusinessName = profile?.BusinessName ?? "",
                PlanName = plan?.PlanName ?? "",
                InsuranceTypeName = plan?.InsuranceType?.TypeName ?? "",
                SelectedCoverageAmount = policy.SelectedCoverageAmount
            };
        }

        public async Task<InvoiceDto> GetInvoiceAsync(string paymentId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null) throw new KeyNotFoundException("Payment not found.");

            var policy = await _policyRepository.GetByIdAsync(payment.PolicyId);
            if (policy == null) throw new KeyNotFoundException("Policy not found.");

            var plan = await _planRepository.GetByIdAsync(policy.PlanId);
            var user = await _userRepository.GetByIdAsync(policy.UserId);
            var profile = !string.IsNullOrEmpty(policy.BusinessProfileId)
                ? await _profileRepository.GetByIdAsync(policy.BusinessProfileId)
                : null;

            decimal annualPremium = (policy.SelectedCoverageAmount / 100m) * 5m;

            return new InvoiceDto
            {
                InvoiceNumber = payment.InvoiceNumber,
                PolicyNumber = policy.PolicyNumber,
                CustomerName = user?.FullName ?? "",
                BusinessName = profile?.BusinessName ?? "",
                InsuranceTypeName = plan?.InsuranceType?.TypeName ?? "",
                PlanName = plan?.PlanName ?? "",
                SelectedCoverageAmount = policy.SelectedCoverageAmount,
                AnnualPremium = annualPremium,
                InstallmentAmount = payment.Amount,
                InstallmentNumber = payment.InstallmentNumber,
                TotalInstallments = payment.TotalInstallments,
                PaymentFrequency = payment.PaymentFrequency,
                PaymentMode = payment.PaymentMode,
                PaymentDate = payment.PaymentDate,
                DueDate = payment.DueDate,
                TransactionId = payment.TransactionId,
                Status = payment.Status.ToString()
            };
        }

        public async Task<PaymentScheduleDto> GetPaymentScheduleAsync(string policyId)
        {
            var payments = (await _paymentRepository.FindAsync(p => p.PolicyId == policyId))
                .OrderBy(p => p.InstallmentNumber).ToList();

            var policy = await _policyRepository.GetByIdAsync(policyId);
            if (policy == null) throw new KeyNotFoundException("Policy not found.");

            decimal annualPremium = (policy.SelectedCoverageAmount / 100m) * 5m;
            var first = payments.FirstOrDefault();

            int totalInstallments = first?.TotalInstallments ?? 0;
            decimal installmentAmount = first?.Amount ?? 0;
            string frequency = first?.PaymentFrequency ?? policy.PaymentFrequency;

            if (first == null)
            {
                switch (frequency)
                {
                    case "Monthly":
                        totalInstallments = 12;
                        installmentAmount = Math.Round(annualPremium / 12m, 2);
                        break;
                    case "Quarterly":
                        totalInstallments = 4;
                        installmentAmount = Math.Round(annualPremium / 4m, 2);
                        break;
                    case "HalfYearly":
                        totalInstallments = 2;
                        installmentAmount = Math.Round(annualPremium / 2m, 2);
                        break;
                    case "Annually":
                        totalInstallments = 1;
                        installmentAmount = annualPremium;
                        break;
                }
            }

            return new PaymentScheduleDto
            {
                PolicyId = policyId,
                PolicyNumber = policy.PolicyNumber,
                AnnualPremium = annualPremium,
                InstallmentAmount = installmentAmount,
                PaymentFrequency = frequency,
                TotalInstallments = totalInstallments,
                Schedule = payments.Select(p => new InstallmentDto
                {
                    PaymentId = p.Id,
                    InstallmentNumber = p.InstallmentNumber,
                    InvoiceNumber = p.InvoiceNumber,
                    Amount = p.Amount,
                    DueDate = p.DueDate,
                    Status = p.Status == PaymentStatus.Completed ? "Paid"
                        : (p.DueDate < DateTime.UtcNow ? "Overdue" : "Pending"),
                    PaidAt = p.Status == PaymentStatus.Completed ? p.PaymentDate : null
                }).ToList()
            };
        }

        public async Task<List<PaymentResponseDto>> GetAllPaymentsByPolicyAsync(string policyId)
        {
            var payments = (await _paymentRepository.FindAsync(p => p.PolicyId == policyId))
                .OrderBy(p => p.InstallmentNumber).ToList();

            var policy = await _policyRepository.GetByIdAsync(policyId);
            var plan = policy != null ? await _planRepository.GetByIdAsync(policy.PlanId) : null;
            var user = policy != null ? await _userRepository.GetByIdAsync(policy.UserId) : null;
            var profile = (policy != null && !string.IsNullOrEmpty(policy.BusinessProfileId))
                ? await _profileRepository.GetByIdAsync(policy.BusinessProfileId)
                : null;

            decimal annualPremium = policy != null ? (policy.SelectedCoverageAmount / 100m) * 5m : 0;

            return payments.Select(p => new PaymentResponseDto
            {
                Id = p.Id,
                PolicyId = p.PolicyId,
                PolicyNumber = policy?.PolicyNumber ?? "",
                InvoiceNumber = p.InvoiceNumber,
                Amount = p.Amount,
                AnnualPremium = annualPremium,
                TotalPremium = annualPremium,
                PaymentFrequency = p.PaymentFrequency,
                PaymentMode = p.PaymentMode,
                InstallmentNumber = p.InstallmentNumber,
                TotalInstallments = p.TotalInstallments,
                DueDate = p.DueDate,
                PaymentDate = p.PaymentDate,
                Status = p.Status.ToString(),
                TransactionId = p.TransactionId,
                CustomerName = user?.FullName ?? "",
                BusinessName = profile?.BusinessName ?? "",
                PlanName = plan?.PlanName ?? "",
                InsuranceTypeName = plan?.InsuranceType?.TypeName ?? "",
                SelectedCoverageAmount = policy?.SelectedCoverageAmount ?? 0
            }).ToList();
        }

        // Keep backward compatibility
        public async Task<PaymentDto?> GetPaymentByIdAsync(string id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null) return null;

            var policy = await _policyRepository.GetByIdAsync(payment.PolicyId);
            return MapToDto(payment, policy?.PolicyNumber ?? "Unknown");
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByPolicyAsync(string policyId)
        {
            var payments = await _paymentRepository.FindAsync(p => p.PolicyId == policyId);
            var policy = await _policyRepository.GetByIdAsync(policyId);
            return payments.Select(p => MapToDto(p, policy?.PolicyNumber ?? "Unknown"));
        }

        private static PaymentDto MapToDto(Payment payment, string policyNumber) => new()
        {
            Id = payment.Id,
            PolicyId = payment.PolicyId,
            PolicyNumber = policyNumber,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            Status = payment.Status.ToString(),
            TransactionId = payment.TransactionId,
            InvoiceInfo = payment.InvoiceNumber
        };
    }
}
