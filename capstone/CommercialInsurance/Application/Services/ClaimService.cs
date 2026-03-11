// Manages claim filing, status transitions (Submitted → UnderReview → Approved/Rejected → Settled), document uploads, and notifications.
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGenericRepository<ClaimHistoryLog> _historyRepository;
        private readonly IGenericRepository<Document> _documentRepository;
        private readonly IBusinessProfileRepository _profileRepository;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
        private readonly INotificationService _notificationService;

        public ClaimService(
            IClaimRepository claimRepository,
            IPolicyRepository policyRepository,
            IUserRepository userRepository,
            IGenericRepository<ClaimHistoryLog> historyRepository,
            IGenericRepository<Document> documentRepository,
            IBusinessProfileRepository profileRepository,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env,
            INotificationService notificationService)
        {
            _claimRepository = claimRepository;
            _policyRepository = policyRepository;
            _userRepository = userRepository;
            _historyRepository = historyRepository;
            _documentRepository = documentRepository;
            _profileRepository = profileRepository;
            _env = env;
            _notificationService = notificationService;
        }

        // Generates a sequential claim ID with the prefix 'clm' (e.g. clm1, clm2, ...)
        private async Task<string> GenerateClaimId()
        {
            var all = await _claimRepository.GetAllAsync();
            var count = all.Count();
            return $"clm{count + 1}";
        }

        // Generates a sequential document ID for claim-attached files (cdoc1, cdoc2, ...)
        private async Task<string> GenerateDocId()
        {
            var all = await _documentRepository.GetAllAsync();
            var count = all.Count();
            return $"cdoc{count + 1}";
        }

        // Generates a sequential history log ID (clog1, clog2, ...) for audit trail entries
        private async Task<string> GenerateHistoryId()
        {
            var all = await _historyRepository.GetAllAsync();
            var count = all.Count();
            return $"clog{count + 1}";
        }

        // Files a new insurance claim against an active policy.
        // Validates the policy status, file types/sizes, and remaining coverage before creating the claim.
        // After creation, uploads evidence files and creates the initial audit log entry.
        public async Task<ClaimDto> FileClaimAsync(CreateClaimDto dto, string policyId, IEnumerable<Microsoft.AspNetCore.Http.IFormFile> files)
        {
            var policy = await _policyRepository.GetByIdAsync(policyId);
            if (policy == null) throw new Domain.Exceptions.NotFoundException("Policy not found.");

            // Claims can only be filed on Active policies — not pending, expired, or rejected ones
            if (policy.Status != PolicyStatus.Active)
                throw new Domain.Exceptions.BusinessRuleException("Claims can only be filed on active policies.");

            // At least one supporting document is mandatory for a claim
            if (files == null || !files.Any())
                throw new Domain.Exceptions.ValidationException("At least one supporting document is required.");

            //  FILE VALIDATIONS 
            // Rules: max 5 files, max 5 MB each, only PDF/JPG/JPEG/PNG accepted
            const long maxFileSize = 5 * 1024 * 1024; // 5 MB in bytes
            var allowedTypes = new[] { ".pdf", ".jpg", ".jpeg", ".png",".docx" };

            if (files.Count() > 5)
                throw new Domain.Exceptions.ValidationException("Maximum 5 documents allowed per claim.");

            foreach (var file in files)
            {
                if (file.Length > maxFileSize)
                    throw new Domain.Exceptions.ValidationException($"File {file.FileName} exceeds 5MB limit.");

                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!allowedTypes.Contains(ext))
                    throw new Domain.Exceptions.ValidationException($"File type {ext} is not allowed.");
            }

            //  REMAINING COVERAGE CHECK 
            // Sum up all non-rejected claims already filed on this policy.
            // If the new claim amount exceeds the remaining coverage, reject it — the policy cannot pay out more than its limit.
            var existingClaims = await _claimRepository.FindAsync(c => c.PolicyId == policyId && c.Status != ClaimStatus.Rejected);
            var usedCoverage = existingClaims.Sum(c => c.ClaimAmount);
            var remainingCoverage = policy.SelectedCoverageAmount - usedCoverage;

            if (dto.ClaimAmount > remainingCoverage)
            {
                throw new Domain.Exceptions.BusinessRuleException($"Claim amount cannot exceed the remaining coverage amount of {remainingCoverage:C}.");
            }

            // Assignment Logic: Use the claims officer already assigned to this customer.
            // This ensures continuity — the same officer handles all of a customer's claims.
            var customer = await _userRepository.GetByIdAsync(policy.UserId);
            var officerId = customer?.AssignedClaimsOfficerId;

            if (string.IsNullOrEmpty(officerId))
            {
                // Fallback for legacy accounts that do not have a pre-assigned officer.
                // Select the officer with the fewest currently open (Submitted or UnderReview) claims.
                var officers = await _userRepository.FindAsync(u => u.Role == UserRole.ClaimsOfficer);
                if (!officers.Any())
                    throw new InvalidOperationException("No claims officers available for assignment.");

                var allClaims = await _claimRepository.GetAllAsync();
                officerId = officers
                    .Select(o => new
                    {
                        OfficerId = o.Id,
                        Count = allClaims.Count(c => c.ClaimsOfficerId == o.Id && (c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.UnderReview))
                    })
                    .OrderBy(x => x.Count)
                    .First().OfficerId;

                // Persist the assignment so future claims for this customer go to the same officer
                if (customer != null)
                {
                    customer.AssignedClaimsOfficerId = officerId;
                    await _userRepository.UpdateAsync(customer);
                }
            }

            var claim = new Claim
            {
                Id = await GenerateClaimId(),
                ClaimNumber = $"CLM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                PolicyId = policyId,
                Description = dto.Description,
                ClaimAmount = dto.ClaimAmount,
                Status = ClaimStatus.Submitted,
                ClaimsOfficerId = officerId
            };

            await _claimRepository.AddAsync(claim);

            // Process Files
            var safeWebRoot = string.IsNullOrEmpty(_env.WebRootPath) ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot") : _env.WebRootPath;
            var uploadsFolder = Path.Combine(safeWebRoot, "uploads", policy.UserId, "claims", claim.Id);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            foreach (var file in files)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                await _documentRepository.AddAsync(new Document
                {
                    Id = await GenerateDocId(),
                    ClaimId = claim.Id,
                    FileName = file.FileName,
                    FilePath = $"/uploads/{policy.UserId}/claims/{claim.Id}/{fileName}",
                    FileType = file.ContentType,
                    FileSize = file.Length
                });
            }

            // Create the initial audit log entry showing the claim was filed
            await _historyRepository.AddAsync(new ClaimHistoryLog
            {
                Id = await GenerateHistoryId(),
                ClaimId = claim.Id,
                Status = ClaimStatus.Submitted,
                Remarks = "Claim filed with supporting documents.",
                ChangedByUserId = policy.UserId, // The customer is the one who filed it
                ChangedAt = DateTime.UtcNow
            });

            if (!string.IsNullOrEmpty(claim.ClaimsOfficerId))
            {
                await _notificationService.CreateNotificationAsync(
                    claim.ClaimsOfficerId,
                    "New Claim Assigned",
                    $"Claim {claim.ClaimNumber} has been submitted and assigned to you.",
                    NotificationType.Claim.ToString(),
                    claim.Id
                );
            }

            return await GetClaimByIdAsync(claim.Id) ?? throw new Exception("Error retrieving saved claim.");
        }

        // Updates the claim's status (UnderReview, Approved, Rejected, Settled, etc.).
        // Always creates an audit history log entry so reviewers can trace every status change.
        // Sends a real-time notification to the policy owner after every status change.
        public async Task<ClaimDto> UpdateClaimStatusAsync(string claimId, UpdateClaimStatusDto dto, string officerId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null) throw new Domain.Exceptions.NotFoundException("Claim not found.");

            // Validate that the supplied status string maps to a known ClaimStatus enum value
            if (!Enum.TryParse<ClaimStatus>(dto.Status, true, out var newStatus))
                throw new Domain.Exceptions.ValidationException($"Invalid status: {dto.Status}");

            claim.Status = newStatus;
            // Record the resolution timestamp when the claim reaches a terminal state
            if (newStatus == ClaimStatus.Approved || newStatus == ClaimStatus.Rejected || newStatus == ClaimStatus.Settled)
                claim.ResolvedAt = DateTime.UtcNow;

            await _claimRepository.UpdateAsync(claim);

            // Add an audit trail entry recording who changed the status, when, and with what remarks
            await _historyRepository.AddAsync(new ClaimHistoryLog
            {
                Id = await GenerateHistoryId(),
                ClaimId = claim.Id,
                Status = newStatus,
                Remarks = dto.Remarks,
                ChangedByUserId = officerId, // The claims officer performing the update
                ChangedAt = DateTime.UtcNow
            });

            // Notify the policy owner (customer) of the status change
            var policy = await _policyRepository.GetByIdAsync(claim.PolicyId);
            if (policy != null)
            {
                await _notificationService.CreateNotificationAsync(
                    policy.UserId,
                    $"Claim {newStatus}",
                    $"Your claim {claim.ClaimNumber} status has been updated to {newStatus}.",
                    NotificationType.Claim.ToString(),
                    claim.Id
                );
            }

            return await GetClaimByIdAsync(claimId) ?? throw new Exception("Error retrieving updated claim.");
        }

        public async Task<ClaimDto?> GetClaimByIdAsync(string id)
        {
            var claim = await _claimRepository.GetByIdAsync(id);
            if (claim == null) return null;
            var results = await MapClaimsAsync(new[] { claim });
            return results.FirstOrDefault();
        }

        public async Task<IEnumerable<ClaimDto>> GetClaimsByPolicyAsync(string policyId)
        {
            var claims = await _claimRepository.GetClaimsByPolicyIdAsync(policyId);
            return await MapClaimsAsync(claims);
        }

        public async Task<IEnumerable<ClaimDto>> GetClaimsByOfficerAsync(string officerId)
        {
            var claims = await _claimRepository.GetClaimsByOfficerIdAsync(officerId);
            return await MapClaimsAsync(claims);
        }

        public async Task<IEnumerable<ClaimDto>> GetAllClaimsAsync()
        {
            var claims = await _claimRepository.GetAllAsync();
            return await MapClaimsAsync(claims);
        }

        private async Task<IEnumerable<ClaimDto>> MapClaimsAsync(IEnumerable<Claim> claims)
        {
            var result = new List<ClaimDto>();
            foreach (var c in claims)
            {
                var policy = await _policyRepository.GetByIdAsync(c.PolicyId);
                var officer = !string.IsNullOrEmpty(c.ClaimsOfficerId) ? await _userRepository.GetByIdAsync(c.ClaimsOfficerId) : null;
                var allHistory = await _historyRepository.FindAsync(h => h.ClaimId == c.Id);
                var historyLogs = new List<ClaimHistoryLogDto>();

                foreach (var h in allHistory.OrderBy(x => x.ChangedAt))
                {
                    var changedBy = await _userRepository.GetByIdAsync(h.ChangedByUserId);
                    historyLogs.Add(new ClaimHistoryLogDto
                    {
                        Id = h.Id,
                        ClaimId = h.ClaimId,
                        Status = h.Status.ToString(),
                        Remarks = h.Remarks,
                        ChangedByUserId = h.ChangedByUserId,
                        ChangedByUserName = changedBy?.FullName ?? "System",
                        ChangedAt = h.ChangedAt
                    });
                }

                var allDocs = await _documentRepository.FindAsync(d => d.ClaimId == c.Id);
                var documents = allDocs.Select(d => new DocumentDto
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    FileType = d.FileType,
                    FileSize = d.FileSize,
                    FilePath = d.FilePath,
                    UploadedAt = d.UploadedAt
                }).ToList();

                var profile = (policy != null && !string.IsNullOrEmpty(policy.BusinessProfileId))
                    ? await _profileRepository.GetByIdAsync(policy.BusinessProfileId)
                    : null;

                result.Add(new ClaimDto
                {
                    Id = c.Id,
                    ClaimNumber = c.ClaimNumber,
                    PolicyId = c.PolicyId,
                    PolicyNumber = policy?.PolicyNumber ?? "",
                    Description = c.Description,
                    ClaimAmount = c.ClaimAmount,
                    Status = c.Status.ToString(),
                    ClaimsOfficerId = c.ClaimsOfficerId,
                    ClaimsOfficerName = officer?.FullName,
                    CreatedAt = c.CreatedAt,
                    ResolvedAt = c.ResolvedAt,
                    BusinessName = profile?.BusinessName ?? "",
                    Industry = profile?.Industry ?? "",
                    EmployeeCount = profile?.EmployeeCount ?? 0,
                    AnnualRevenue = profile?.AnnualRevenue ?? 0,
                    History = historyLogs,
                    Documents = documents
                });
            }
            return result;
        }
    }
}
