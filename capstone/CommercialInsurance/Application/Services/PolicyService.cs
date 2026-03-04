// Provides core functionality and structures for the application.
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IPlanRepository _planRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBusinessProfileRepository _profileRepository;
        private readonly IGenericRepository<Document> _documentRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IGenericRepository<Domain.Entities.ClaimHistoryLog> _historyRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly INotificationService _notificationService;

        public PolicyService(
            IPolicyRepository policyRepository,
            IPlanRepository planRepository,
            IUserRepository userRepository,
            IBusinessProfileRepository profileRepository,
            IGenericRepository<Document> documentRepository,
            IClaimRepository claimRepository,
            IPaymentRepository paymentRepository,
            IGenericRepository<Domain.Entities.ClaimHistoryLog> historyRepository,
            IWebHostEnvironment environment,
            INotificationService notificationService)
        {
            _policyRepository = policyRepository;
            _planRepository = planRepository;
            _userRepository = userRepository;
            _profileRepository = profileRepository;
            _documentRepository = documentRepository;
            _claimRepository = claimRepository;
            _paymentRepository = paymentRepository;
            _historyRepository = historyRepository;
            _environment = environment;
            _notificationService = notificationService;
        }

        private async Task<string> GenerateNextId()
        {
            return Guid.NewGuid().ToString();
        }

        private async Task<string> GenerateDocId()
        {
            return Guid.NewGuid().ToString();
        }

        public async Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto dto, string createdByUserId, IEnumerable<IFormFile> documents, string? agentId = null)
        {
            var profile = await _profileRepository.GetByIdAsync(dto.BusinessProfileId);
            if (profile == null)
                throw new KeyNotFoundException("Business Profile not found.");

            if (!profile.IsProfileCompleted)
                throw new InvalidOperationException("Business Profile is not complete.");

            var plan = await _planRepository.GetByIdAsync(dto.PlanId);
            if (plan == null) throw new KeyNotFoundException("Plan not found.");

            var creator = await _userRepository.GetByIdAsync(createdByUserId);
            if (creator == null) throw new KeyNotFoundException("Creator not found.");

            var customer = await _userRepository.GetByIdAsync(profile.UserId);
            if (customer == null) throw new KeyNotFoundException("Customer not found.");

            var assignedAgentId = customer.AssignedAgentId;
            if (string.IsNullOrEmpty(assignedAgentId))
            {
                assignedAgentId = await AutoAssignAgentByWorkloadAsync();
                if (!string.IsNullOrEmpty(assignedAgentId))
                {
                    customer.AssignedAgentId = assignedAgentId;
                    await _userRepository.UpdateAsync(customer);
                }
            }

            // Always override with assigned agent
            agentId = assignedAgentId;

            var existingPolicies = await _policyRepository.GetPoliciesByUserIdAsync(customer.Id);
            bool hasDuplicate = existingPolicies.Any(p =>
                p.PlanId == dto.PlanId &&
                p.BusinessProfileId == dto.BusinessProfileId &&
                (p.Status == PolicyStatus.Active ||
                 p.Status == PolicyStatus.Approved ||
                 p.Status == PolicyStatus.PendingReview ||
                 p.Status == PolicyStatus.QuoteAccepted));

            if (hasDuplicate)
            {
                throw new InvalidOperationException("This business already has an active or pending policy for this plan.");
            }

            // Document Validation
            var files = documents?.ToList() ?? new List<IFormFile>();
            if (!files.Any()) throw new ArgumentException("Minimum 1 document is required.");
            if (files.Count > 5) throw new ArgumentException("Maximum 5 documents allowed.");

            string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                    throw new ArgumentException($"File type {ext} not allowed.");
                if (file.Length > 5 * 1024 * 1024)
                    throw new ArgumentException("Max file size (5MB) exceeded.");
            }

            Policy policy;
            bool isNew = false;
            if (!string.IsNullOrEmpty(dto.QuoteId))
            {
                policy = await _policyRepository.GetByIdAsync(dto.QuoteId) ?? throw new KeyNotFoundException("Quote not found.");
                policy.SelectedCoverageAmount = dto.SelectedCoverageAmount;
                policy.PremiumAmount = dto.PremiumAmount;
                policy.PaymentFrequency = dto.PaymentFrequency;
                policy.StartDate = dto.StartDate;
                policy.EndDate = dto.EndDate;
                policy.Status = PolicyStatus.PendingReview;
                policy.CommissionAmount = Math.Round(dto.PremiumAmount * 0.10m, 2);
                policy.AutoRenew = dto.AutoRenew;
                await _policyRepository.UpdateAsync(policy);
            }
            else
            {
                policy = new Policy
                {
                    Id = await GenerateNextId(),
                    PolicyNumber = $"PRQ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    UserId = profile.UserId,
                    PlanId = dto.PlanId,
                    BusinessProfileId = dto.BusinessProfileId,
                    AgentId = agentId,
                    CreatedByUserId = createdByUserId,
                    SelectedCoverageAmount = dto.SelectedCoverageAmount,
                    PremiumAmount = dto.PremiumAmount,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Status = PolicyStatus.PendingReview,
                    CommissionAmount = Math.Round(dto.PremiumAmount * 0.10m, 2),
                    CommissionStatus = CommissionStatus.Pending,
                    AutoRenew = dto.AutoRenew,
                    PaymentFrequency = dto.PaymentFrequency
                };
                isNew = true;
            }

            if (isNew)
            {
                await _policyRepository.AddAsync(policy);

                if (!string.IsNullOrEmpty(agentId))
                {
                    await _notificationService.CreateNotificationAsync(
                        agentId,
                        "New Policy Request",
                        $"A new policy request {policy.PolicyNumber} has been submitted and assigned to you.",
                        NotificationType.Policy.ToString(),
                        policy.Id
                    );
                }

                // Notify Admin
                await NotifyAdminAsync(
                    "New Policy Request",
                    $"Customer {customer.FullName} submitted a new policy request {policy.PolicyNumber}.",
                    policy.Id
                );
            }

            // Save Documents
            string uploadPath = Path.Combine(_environment.WebRootPath, "uploads", policy.UserId, "policies", policy.Id);
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            var existingFileNames = new HashSet<string>();
            foreach (var file in files)
            {
                string safeFileName = Path.GetFileName(file.FileName);
                if (existingFileNames.Contains(safeFileName))
                    safeFileName = $"{Guid.NewGuid()}_{safeFileName}";

                existingFileNames.Add(safeFileName);
                string filePath = Path.Combine(uploadPath, safeFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                await _documentRepository.AddAsync(new Document
                {
                    Id = await GenerateDocId(),
                    PolicyId = policy.Id,
                    FileName = file.FileName,
                    FilePath = $"/uploads/{policy.UserId}/policies/{policy.Id}/{safeFileName}",
                    FileType = file.ContentType,
                    FileSize = file.Length
                });
            }

            return await GetPolicyByIdAsync(policy.Id) ?? throw new Exception("Error retrieving policy.");
        }

        public async Task<IEnumerable<PolicyDto>> GetPoliciesByUserAsync(string userId)
        {
            var policies = await _policyRepository.GetPoliciesByUserIdAsync(userId);
            return await MapPoliciesAsync(policies);
        }

        public async Task<IEnumerable<PolicyDto>> GetPoliciesByAgentAsync(string agentId)
        {
            var policies = await _policyRepository.GetPoliciesByAgentIdAsync(agentId);
            return await MapPoliciesAsync(policies);
        }

        public async Task<PolicyDto?> GetPolicyByIdAsync(string id)
        {
            var policy = await _policyRepository.GetByIdAsync(id);
            if (policy == null) return null;

            var user = await _userRepository.GetByIdAsync(policy.UserId);
            var plan = await _planRepository.GetByIdAsync(policy.PlanId);
            var profile = !string.IsNullOrEmpty(policy.BusinessProfileId)
                ? await _profileRepository.GetByIdAsync(policy.BusinessProfileId)
                : null;
            var documents = await _documentRepository.FindAsync(d => d.PolicyId == id);

            var claims = await _claimRepository.FindAsync(c => c.PolicyId == id && c.Status != ClaimStatus.Rejected);
            var remainingCoverage = policy.SelectedCoverageAmount - claims.Sum(c => c.ClaimAmount);

            var agentName = "";
            if (!string.IsNullOrEmpty(policy.AgentId))
            {
                var agent = await _userRepository.GetByIdAsync(policy.AgentId);
                agentName = agent?.FullName ?? "";
            }

            return new PolicyDto
            {
                Id = policy.Id,
                PolicyNumber = policy.PolicyNumber,
                UserId = policy.UserId,
                CustomerName = user?.FullName ?? "",
                PlanId = policy.PlanId,
                PlanName = plan?.PlanName ?? "",
                BusinessProfileId = policy.BusinessProfileId,
                BusinessName = profile?.BusinessName ?? "",
                Industry = profile?.Industry ?? "",
                EmployeeCount = profile?.EmployeeCount ?? 0,
                AnnualRevenue = profile?.AnnualRevenue ?? 0,
                AgentId = policy.AgentId,
                AgentName = agentName,
                SelectedCoverageAmount = policy.SelectedCoverageAmount,
                RemainingCoverageAmount = remainingCoverage,
                PremiumAmount = policy.PremiumAmount,
                StartDate = policy.StartDate,
                EndDate = policy.EndDate,
                Status = policy.Status.ToString(),
                RejectionReason = policy.RejectionReason,
                CommissionAmount = policy.CommissionAmount,
                CommissionStatus = policy.CommissionStatus.ToString(),
                AutoRenew = policy.AutoRenew,
                CreatedAt = policy.CreatedAt,
                Documents = documents.Select(d => new DocumentDto
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    FileType = d.FileType,
                    FileSize = d.FileSize,
                    UploadedAt = d.UploadedAt
                }).ToList()
            };
        }

        public async Task<IEnumerable<PolicyDto>> GetAllPoliciesAsync()
        {
            var policies = await _policyRepository.GetAllAsync();
            return await MapPoliciesAsync(policies);
        }

        private async Task<IEnumerable<PolicyDto>> MapPoliciesAsync(IEnumerable<Policy> policies)
        {
            var result = new List<PolicyDto>();
            foreach (var p in policies)
            {
                var user = await _userRepository.GetByIdAsync(p.UserId);
                var plan = await _planRepository.GetByIdAsync(p.PlanId);
                var creator = await _userRepository.GetByIdAsync(p.CreatedByUserId);

                var agentName = "";
                if (p.AgentId != null)
                {
                    var agent = await _userRepository.GetByIdAsync(p.AgentId);
                    agentName = agent?.FullName ?? "";
                }

                var profile = !string.IsNullOrEmpty(p.BusinessProfileId)
                    ? await _profileRepository.GetByIdAsync(p.BusinessProfileId)
                    : null;
                var documents = await _documentRepository.FindAsync(d => d.PolicyId == p.Id);

                var claims = await _claimRepository.FindAsync(c => c.PolicyId == p.Id && c.Status != ClaimStatus.Rejected);
                var remainingCoverage = p.SelectedCoverageAmount - claims.Sum(c => c.ClaimAmount);

                var status = p.Status;
                if (status == PolicyStatus.Active && p.EndDate < DateTime.UtcNow)
                {
                    status = PolicyStatus.Expired;
                    p.Status = PolicyStatus.Expired;
                    await _policyRepository.UpdateAsync(p);
                }

                result.Add(new PolicyDto
                {
                    Id = p.Id,
                    PolicyNumber = p.PolicyNumber,
                    UserId = p.UserId,
                    CustomerName = user?.FullName ?? "",
                    PlanId = p.PlanId,
                    PlanName = plan?.PlanName ?? "",
                    BusinessProfileId = p.BusinessProfileId,
                    BusinessName = profile?.BusinessName ?? "",
                    Industry = profile?.Industry ?? "",
                    EmployeeCount = profile?.EmployeeCount ?? 0,
                    AnnualRevenue = profile?.AnnualRevenue ?? 0,
                    AgentId = p.AgentId,
                    AgentName = agentName,
                    CreatedByUserId = p.CreatedByUserId,
                    CreatedByName = creator?.FullName ?? "",
                    SelectedCoverageAmount = p.SelectedCoverageAmount,
                    RemainingCoverageAmount = remainingCoverage,
                    PremiumAmount = p.PremiumAmount,
                    PaymentFrequency = p.PaymentFrequency,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Status = p.Status.ToString(),
                    RejectionReason = p.RejectionReason,
                    CommissionAmount = p.CommissionAmount,
                    CommissionStatus = p.CommissionStatus.ToString(),
                    AutoRenew = p.AutoRenew,
                    CreatedAt = p.CreatedAt,
                    Documents = documents.Select(d => new DocumentDto
                    {
                        Id = d.Id,
                        FileName = d.FileName,
                        FileType = d.FileType,
                        FileSize = d.FileSize,
                        FilePath = d.FilePath,
                        UploadedAt = d.UploadedAt
                    }).ToList()
                });
            }
            return result;
        }

        public async Task<PremiumCalculationDto> CalculatePremiumAsync(PremiumCalculationRequestDto request)
        {
            var plan = await _planRepository.GetByIdAsync(request.PlanId);
            if (plan == null) throw new KeyNotFoundException("Plan not found.");

            BusinessProfile? profile;
            if (!string.IsNullOrEmpty(request.BusinessProfileId))
            {
                profile = await _profileRepository.GetByIdAsync(request.BusinessProfileId);
            }
            else
            {
                profile = await _profileRepository.GetByUserIdAsync(request.CustomerId);
            }

            if (profile == null || !profile.IsProfileCompleted)
            {
                throw new InvalidOperationException("Please complete business profile before generating quote.");
            }

            if (plan.MaxCoverageAmount > 0 && (request.SelectedCoverageAmount < plan.MinCoverageAmount || request.SelectedCoverageAmount > plan.MaxCoverageAmount))
            {
                throw new ArgumentOutOfRangeException(nameof(request.SelectedCoverageAmount), "Selected coverage amount must be within the plan's allowed limits.");
            }

            decimal industryMultiplier = profile.Industry.ToLower() switch
            {
                "technology" => 1.1m,
                "manufacturing" => 1.5m,
                "healthcare" => 1.3m,
                "retail" => 1.1m,
                "finance" => 1.2m,
                _ => 1.0m
            };

            decimal employeeMultiplier = profile.EmployeeCount switch
            {
                <= 10 => 1.0m,
                <= 50 => 1.2m,
                <= 200 => 1.5m,
                _ => 2.0m
            };

            decimal revenueMultiplier = profile.AnnualRevenue switch
            {
                < 100000 => 1.0m,
                < 500000 => 1.2m,
                < 2000000 => 1.5m,
                _ => 2.0m
            };

            decimal basePremium = plan.BasePremium > 0 ? plan.BasePremium : 50m;
            // ───── Insurance Calculation Engine ─────
            // We use a base rate (e.g. 0.1% to 2% of coverage) instead of just multiplying base premium
            decimal baseRate = (plan.BasePremium > 0 ? plan.BasePremium : 150m) / 1000m; // 0.15% if default 150
            if (baseRate > 0.05m) baseRate = 0.02m; // Cap at 2% for base

            decimal coveragePremium = request.SelectedCoverageAmount * baseRate;

            decimal multiplierSum = industryMultiplier * employeeMultiplier * revenueMultiplier;
            decimal totalAnnualPremium = coveragePremium * multiplierSum;

            // Commission calculation (10% standard)
            decimal commissionPercentage = 0.10m;
            decimal annualCommission = totalAnnualPremium * commissionPercentage;
            decimal finalAnnualPremium = totalAnnualPremium + annualCommission;

            // Frequencies
            decimal monthly = Math.Round(finalAnnualPremium / 12, 2);
            decimal yearly = Math.Round(finalAnnualPremium * 0.95m, 2); // 5% discount for yearly

            decimal finalPremium = (request.PaymentFrequency?.ToLower() == "yearly") ? yearly : monthly;

            return new PremiumCalculationDto
            {
                QuoteId = "",
                QuoteNumber = "",
                BasePremium = (plan.BasePremium > 0 ? plan.BasePremium : 150m),
                CoveragePremium = Math.Round(coveragePremium, 2),
                IndustryMultiplier = industryMultiplier,
                EmployeeCountMultiplier = employeeMultiplier,
                RevenueMultiplier = revenueMultiplier,
                AgentCommissionPercentage = 10m,
                AgentCommissionAmount = Math.Round(finalPremium * 0.10m, 2),
                MonthlyPremium = monthly,
                YearlyPremium = yearly,
                PaymentFrequency = request.PaymentFrequency ?? "Monthly",
                FinalPremium = finalPremium
            };
        }

        private async Task<string?> AutoAssignAgentByWorkloadAsync()
        {
            var agents = (await _userRepository.FindAsync(u => u.Role == UserRole.Agent)).ToList();
            if (!agents.Any()) return null;

            var workloads = new List<(string AgentId, int Count)>();
            foreach (var agent in agents)
            {
                var agentPolicies = await _policyRepository.FindAsync(p => p.AgentId == agent.Id);
                workloads.Add((agent.Id, agentPolicies.Count()));
            }
            return workloads.OrderBy(w => w.Count).First().AgentId;
        }

        private async Task NotifyAdminAsync(string title, string message, string? referenceId = null)
        {
            var admins = await _userRepository.FindAsync(u => u.Role == UserRole.Admin);
            foreach (var admin in admins)
            {
                await _notificationService.CreateNotificationAsync(
                    admin.Id,
                    title,
                    message,
                    NotificationType.System.ToString(),
                    referenceId
                );
            }
        }

        public async Task<PolicyDto> ApprovePolicyAsync(string policyId)
        {
            var policy = await _policyRepository.GetByIdAsync(policyId);
            if (policy == null) throw new KeyNotFoundException("Policy not found.");

            if (policy.Status != PolicyStatus.PendingReview)
                throw new InvalidOperationException("Only PendingReview policies can be approved.");

            policy.Status = PolicyStatus.Approved;
            await _policyRepository.UpdateAsync(policy);

            // Notify Customer
            await _notificationService.CreateNotificationAsync(
                policy.UserId,
                "Policy Approved",
                $"Your policy request {policy.PolicyNumber} has been approved.",
                NotificationType.Policy.ToString(),
                policy.Id
            );

            // Notify Admin
            await NotifyAdminAsync(
                "Policy Approved",
                $"Policy {policy.PolicyNumber} has been approved by the agent.",
                policy.Id
            );

            // TODO: Generate Invoice logic here

            return await GetPolicyByIdAsync(policyId) ?? throw new Exception("Error retrieving policy.");
        }

        public async Task<PolicyDto> RejectPolicyAsync(string policyId, RejectPolicyDto dto)
        {
            var policy = await _policyRepository.GetByIdAsync(policyId);
            if (policy == null) throw new KeyNotFoundException("Policy not found.");

            if (policy.Status != PolicyStatus.PendingReview)
                throw new InvalidOperationException("Only PendingReview policies can be rejected.");

            policy.Status = PolicyStatus.Rejected;
            policy.RejectionReason = dto.Reason;
            await _policyRepository.UpdateAsync(policy);

            // Notify Customer
            await _notificationService.CreateNotificationAsync(
                policy.UserId,
                "Policy Rejected",
                $"Your policy request {policy.PolicyNumber} was rejected. Reason: {dto.Reason}",
                NotificationType.Policy.ToString(),
                policy.Id
            );

            // Notify Admin
            await NotifyAdminAsync(
                "Policy Rejected",
                $"Policy {policy.PolicyNumber} has been rejected by the agent. Reason: {dto.Reason}",
                policy.Id
            );

            return await GetPolicyByIdAsync(policyId) ?? throw new Exception("Error retrieving policy.");
        }

        public async Task<PolicyDto> IssuePolicyAsync(string policyId)
        {
            var policy = await _policyRepository.GetByIdAsync(policyId);
            if (policy == null) throw new KeyNotFoundException("Policy not found.");

            // Policy can be activated if Approved (after payment) or QuoteAccepted
            if (policy.Status != PolicyStatus.Approved && policy.Status != PolicyStatus.QuoteAccepted && policy.Status != PolicyStatus.QuoteGenerated)
                throw new InvalidOperationException("Policy must be Approved before activation.");

            policy.Status = PolicyStatus.Active;
            if (policy.PolicyNumber.StartsWith("PRQ-") || policy.PolicyNumber.StartsWith("QTE-"))
            {
                policy.PolicyNumber = "POL-" + Guid.NewGuid().ToString()[..8].ToUpper();
            }

            policy.CommissionStatus = CommissionStatus.Earned;
            await _policyRepository.UpdateAsync(policy);

            // Notify Customer
            await _notificationService.CreateNotificationAsync(
                policy.UserId,
                "Policy Activated",
                $"Your policy {policy.PolicyNumber} is now active! Coverage starts from {policy.StartDate:MMM d, yyyy}.",
                NotificationType.Policy.ToString(),
                policy.Id
            );

            // Notify Admin
            await NotifyAdminAsync(
                "Policy Activated",
                $"Policy {policy.PolicyNumber} has been activated.",
                policy.Id
            );

            return await GetPolicyByIdAsync(policyId) ?? throw new Exception("Error retrieving policy.");
        }

        public async Task<PolicyDto> ToggleAutoRenewAsync(string policyId)
        {
            var policy = await _policyRepository.GetByIdAsync(policyId);
            if (policy == null) throw new KeyNotFoundException("Policy not found.");

            policy.AutoRenew = !policy.AutoRenew;
            await _policyRepository.UpdateAsync(policy);

            return await GetPolicyByIdAsync(policyId) ?? throw new Exception("Error retrieving policy.");
        }

        public async Task<PolicyDto> RenewPolicyAsync(string policyId)
        {
            var oldPolicy = await _policyRepository.GetByIdAsync(policyId);
            if (oldPolicy == null) throw new KeyNotFoundException("Policy not found.");

            if (oldPolicy.Status != PolicyStatus.Active && oldPolicy.Status != PolicyStatus.Expired)
            {
                throw new InvalidOperationException("Only active or expired policies can be renewed.");
            }

            DateTime newStartDate;
            bool isLapsed = oldPolicy.Status == PolicyStatus.Expired || (oldPolicy.Status == PolicyStatus.Active && oldPolicy.EndDate < DateTime.UtcNow);

            if (isLapsed)
            {
                // Case 2: Lapsed Renewal - Coverage gap exists (StartDate = Today)
                newStartDate = DateTime.UtcNow;
            }
            else // Must be PolicyStatus.Active and not yet past EndDate
            {
                var daysUntilExpiry = (oldPolicy.EndDate - DateTime.UtcNow).TotalDays;
                if (daysUntilExpiry > 30)
                {
                    throw new InvalidOperationException(
                        $"Renewal is available 30 days before expiry. Your policy expires on {oldPolicy.EndDate:MMM d, yyyy} ({(int)daysUntilExpiry} days remaining).");
                }
                // Case 1: Early Renewal - No coverage gap (StartDate = EndDate + 1)
                newStartDate = oldPolicy.EndDate.AddDays(1);
            }

            var activePolicies = await _policyRepository.FindAsync(p =>
                p.PreviousPolicyId == policyId &&
                p.Status != PolicyStatus.Rejected &&
                p.Status != PolicyStatus.Cancelled);

            if (activePolicies.Any())
            {
                throw new InvalidOperationException("A renewal for this policy already exists in the system.");
            }

            var plan = await _planRepository.GetByIdAsync(oldPolicy.PlanId);
            var durationMonths = plan?.DurationInMonths ?? 12;

            // Create new policy
            var newPolicy = new Policy
            {
                Id = await GenerateNextId(),
                PolicyNumber = $"RNW-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                UserId = oldPolicy.UserId,
                PlanId = oldPolicy.PlanId,
                BusinessProfileId = oldPolicy.BusinessProfileId,
                AgentId = oldPolicy.AgentId,
                CreatedByUserId = oldPolicy.CreatedByUserId,
                SelectedCoverageAmount = oldPolicy.SelectedCoverageAmount,
                PremiumAmount = oldPolicy.PremiumAmount, 
                StartDate = newStartDate,
                EndDate = newStartDate.AddMonths(durationMonths),
                Status = PolicyStatus.PendingPayment,
                CommissionAmount = oldPolicy.CommissionAmount,
                CommissionStatus = CommissionStatus.Pending,
                AutoRenew = oldPolicy.AutoRenew,
                PaymentFrequency = oldPolicy.PaymentFrequency,
                PreviousPolicyId = oldPolicy.Id,
                IsRenewal = true
            };

            await _policyRepository.AddAsync(newPolicy);

            // Notify Customer
            await _notificationService.CreateNotificationAsync(
                oldPolicy.UserId,
                "Policy Renewed",
                $"Your policy {oldPolicy.PolicyNumber} has been renewed. New policy: {newPolicy.PolicyNumber}.",
                NotificationType.Policy.ToString(),
                newPolicy.Id
            );

            // Notify Agent
            if (!string.IsNullOrEmpty(oldPolicy.AgentId))
            {
                await _notificationService.CreateNotificationAsync(
                    oldPolicy.AgentId,
                    "Policy Renewal",
                    $"Policy {oldPolicy.PolicyNumber} has been renewed as {newPolicy.PolicyNumber}.",
                    NotificationType.Policy.ToString(),
                    newPolicy.Id
                );
            }

            // Notify Admin
            await NotifyAdminAsync(
                "Policy Renewed",
                $"Policy {oldPolicy.PolicyNumber} has been renewed as {newPolicy.PolicyNumber}.",
                newPolicy.Id
            );

            return await GetPolicyByIdAsync(newPolicy.Id) ?? throw new Exception("Error retrieving renewed policy.");
        }

        public async Task<bool> DeletePolicyAsync(string policyId)
        {
            var policy = await _policyRepository.GetByIdAsync(policyId);
            if (policy == null) return false;

            var payments = await _paymentRepository.FindAsync(p => p.PolicyId == policyId);
            foreach (var p in payments) await _paymentRepository.DeleteAsync(p);

            var claims = await _claimRepository.FindAsync(c => c.PolicyId == policyId);
            foreach (var c in claims)
            {
                var history = await _historyRepository.FindAsync(h => h.ClaimId == c.Id);
                foreach (var h in history) await _historyRepository.DeleteAsync(h);

                var claimDocs = await _documentRepository.FindAsync(d => d.ClaimId == c.Id);
                foreach (var d in claimDocs) await _documentRepository.DeleteAsync(d);

                await _claimRepository.DeleteAsync(c);
            }

            var policyDocs = await _documentRepository.FindAsync(d => d.PolicyId == policyId);
            foreach (var d in policyDocs) await _documentRepository.DeleteAsync(d);

            await _policyRepository.DeleteAsync(policy);
            return true;
        }
    }
}
