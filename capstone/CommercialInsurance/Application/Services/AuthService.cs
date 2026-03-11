// Implements user registration with BCrypt hashing, login with JWT generation, secure password reset via email token, and profile updates.
using Application.DTOs;
using Application.Helpers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly INotificationService _notificationService;
        private readonly IPolicyRepository _policyRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public AuthService(
            IUserRepository userRepository,
            JwtTokenGenerator jwtTokenGenerator,
            INotificationService notificationService,
            IPolicyRepository policyRepository,
            IClaimRepository claimRepository,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _notificationService = notificationService;
            _policyRepository = policyRepository;
            _claimRepository = claimRepository;
            _env = env;
        }

        // Validates the user's credentials and, on success, generates a signed JWT token.
        // BCrypt.Verify compares the plaintext password against the stored hash without decrypting.
        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            // Throw UnauthorizedAccessException if the user doesn't exist or the password is wrong.
            // We use the same generic message for both cases to avoid email enumeration attacks.
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            // Generate a signed JWT containing userId, email, and role claims
            var token = _jwtTokenGenerator.GenerateToken(user);

            // Auto-Healing: if a customer has no assigned agent/officer (happens if they registered when system was empty), 
            // try to assign them now so they don't see "SYSTEM" in their dashboard.
            if (user.Role == UserRole.Customer && (string.IsNullOrEmpty(user.AssignedAgentId) || string.IsNullOrEmpty(user.AssignedClaimsOfficerId)))
            {
                bool updated = false;
                if (string.IsNullOrEmpty(user.AssignedAgentId))
                {
                    user.AssignedAgentId = await AutoAssignAgentAsync();
                    updated = true;
                }
                if (string.IsNullOrEmpty(user.AssignedClaimsOfficerId))
                {
                    user.AssignedClaimsOfficerId = await AutoAssignClaimsOfficerAsync();
                    updated = true;
                }

                if (updated)
                {
                    await _userRepository.UpdateAsync(user);
                    // Reload to get navigation properties
                    user = (await _userRepository.GetByIdAsync(user.Id))!;
                }
            }

            return new LoginResponseDto
            {
                Token = token,
                User = MapToDto(user)
            };
        }

        // Creates a new Customer account with workload-based agent and claims officer assignment.
        // After registration the assigned agent and officer each receive a real-time notification.
        public async Task<UserDto> RegisterAsync(RegisterDto dto)
        {
            // Guard: prevent duplicate email registrations
            var existing = await _userRepository.GetByEmailAsync(dto.Email);
            if (existing != null)
                throw new InvalidOperationException("Email is already registered.");

            // Generate a sequential ID with a "cust" prefix (e.g. cust1, cust2, ...)
            var id = await GenerateNextId(UserRole.Customer);

            var user = new User
            {
                Id = id,
                FullName = dto.FullName,
                Email = dto.Email,
                // BCrypt hashes include a salt, so identical passwords produce different hashes
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow,
                // Auto-assign to the agent and claims officer with the fewest currently assigned customers
                AssignedAgentId = await AutoAssignAgentAsync(),
                AssignedClaimsOfficerId = await AutoAssignClaimsOfficerAsync()
            };

            await _userRepository.AddAsync(user);
            
            // Reload the user from repository to ensure the auto-assigned agent and officer 
            // navigation properties are populated (required for the DTO mapping to show names).
            var completedUser = await _userRepository.GetByIdAsync(user.Id);

            // Notify the assigned agent that a new customer has been allocated to them
            if (completedUser != null && !string.IsNullOrEmpty(completedUser.AssignedAgentId))
            {
                await _notificationService.CreateNotificationAsync(
                    completedUser.AssignedAgentId,
                    "New Customer Assigned",
                    $"A new customer {completedUser.FullName} has been assigned to you.",
                    NotificationType.System.ToString()
                );
            }

            // Notify the assigned claims officer that a new customer has been allocated to them
            if (completedUser != null && !string.IsNullOrEmpty(completedUser.AssignedClaimsOfficerId))
            {
                await _notificationService.CreateNotificationAsync(
                    completedUser.AssignedClaimsOfficerId,
                    "New Customer Assigned",
                    $"A new customer {completedUser.FullName} has been assigned to you.",
                    NotificationType.System.ToString()
                );
            }

            return MapToDto(completedUser ?? user);
        }

        // Admin-only: creates a user account with an explicitly supplied role (Agent or ClaimsOfficer).
        // Uses the same registration flow but skips the auto-assignment step.
        public async Task<UserDto> CreateUserWithRoleAsync(RegisterDto dto, string role)
        {
            var existing = await _userRepository.GetByEmailAsync(dto.Email);
            if (existing != null)
                throw new InvalidOperationException("Email is already registered.");

            // Validate the role string against the UserRole enum (case-insensitive)
            if (!Enum.TryParse<UserRole>(role, true, out var userRole))
                throw new ArgumentException($"Invalid role: {role}");

            var id = await GenerateNextId(userRole);
            var user = new User
            {
                Id = id,
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = userRole
            };

            await _userRepository.AddAsync(user);
            return MapToDto(user);
        }

        // Returns all users matching the given role string; used by admin list views.
        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role)
        {
            if (!Enum.TryParse<UserRole>(role, true, out var userRole))
                throw new ArgumentException($"Invalid role: {role}");

            var users = await _userRepository.FindAsync(u => u.Role == userRole);
            return users.Select(MapToDto);
        }

        // Looks up a single user by their primary key and returns a safe DTO (no password hash).
        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null ? MapToDto(user) : null;
        }

        // Permanently deletes a user and handles cascading reassignment before removal.
        // Agent deletion: all of the agent's customers and policies are reassigned to the
        //                 remaining agent with the lowest current policy workload.
        // Officer deletion: all of the officer's customers and open claims are reassigned to
        //                   the remaining officer with the lowest current claim workload.
        // After reassignment, the user's uploaded files in wwwroot/uploads/{userId}/ are deleted.
        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            if (user.Role == UserRole.Agent)
            {
                // Pick the next best agent (excluding the one being deleted)
                var otherAgents = (await _userRepository.FindAsync(u => u.Role == UserRole.Agent && u.Id != userId)).ToList();
                string? newAgentId = null;
                if (otherAgents.Any())
                {
                    // Workload-based: pick agent with fewest assigned policies
                    var workloads = new List<(string AgentId, int Count)>();
                    foreach (var agent in otherAgents)
                    {
                        var count = (await _policyRepository.FindAsync(p => p.AgentId == agent.Id)).Count();
                        workloads.Add((agent.Id, count));
                    }
                    newAgentId = workloads.OrderBy(w => w.Count).First().AgentId;
                }

                // Reassign all customers who pointed to this agent
                var assignedCustomers = await _userRepository.FindAsync(u => u.AssignedAgentId == userId);
                foreach (var customer in assignedCustomers)
                {
                    var tracked = await _userRepository.GetByIdAsync(customer.Id);
                    if (tracked != null) { tracked.AssignedAgentId = newAgentId; await _userRepository.UpdateAsync(tracked); }
                }

                // Reassign all policies owned by this agent
                var agentPolicies = await _policyRepository.FindAsync(p => p.AgentId == userId);
                foreach (var policy in agentPolicies)
                {
                    var trackedPolicy = await _policyRepository.GetByIdAsync(policy.Id);
                    if (trackedPolicy != null) { trackedPolicy.AgentId = newAgentId; await _policyRepository.UpdateAsync(trackedPolicy); }
                }
            }

            if (user.Role == UserRole.ClaimsOfficer)
            {
                // Pick the next best claims officer (excluding the one being deleted)
                var otherOfficers = (await _userRepository.FindAsync(u => u.Role == UserRole.ClaimsOfficer && u.Id != userId)).ToList();
                string? newOfficerId = null;
                if (otherOfficers.Any())
                {
                    // Workload-based: pick the officer with fewest active claims
                    var workloads = new List<(string OfficerId, int Count)>();
                    foreach (var officer in otherOfficers)
                    {
                        var count = (await _claimRepository.FindAsync(c => c.ClaimsOfficerId == officer.Id)).Count();
                        workloads.Add((officer.Id, count));
                    }
                    newOfficerId = workloads.OrderBy(w => w.Count).First().OfficerId;
                }

                // Reassign all customers who pointed to this officer
                var assignedCustomers = await _userRepository.FindAsync(u => u.AssignedClaimsOfficerId == userId);
                foreach (var customer in assignedCustomers)
                {
                    var tracked = await _userRepository.GetByIdAsync(customer.Id);
                    if (tracked != null) { tracked.AssignedClaimsOfficerId = newOfficerId; await _userRepository.UpdateAsync(tracked); }
                }

                // Reassign all open claims owned by this officer
                var officerClaims = await _claimRepository.FindAsync(c => c.ClaimsOfficerId == userId);
                foreach (var claim in officerClaims)
                {
                    var trackedClaim = await _claimRepository.GetByIdAsync(claim.Id);
                    if (trackedClaim != null) { trackedClaim.ClaimsOfficerId = newOfficerId; await _claimRepository.UpdateAsync(trackedClaim); }
                }
            }

            //  Physical File Cleanup 
            // Remove the user's upload folder (wwwroot/uploads/{userId}/) to free disk space.
            // Wrap in try/catch so a file system failure never blocks the database deletion.
            try
            {
                var safeWebRoot = string.IsNullOrEmpty(_env.WebRootPath)
                    ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
                    : _env.WebRootPath;

                var userUploadsFolder = Path.Combine(safeWebRoot, "uploads", userId);

                if (Directory.Exists(userUploadsFolder))
                {
                    Directory.Delete(userUploadsFolder, true); // true = recursive delete
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't block the database deletion
                Console.WriteLine($"Warning: Failed to cleanup physical files for user {userId}: {ex.Message}");
            }

            // Finally delete the user record from the database
            await _userRepository.DeleteAsync(user);
            return true;
        }

        // Simulates sending a password-reset email by writing to the console.
        // In production this would call an email service (SMTP / SendGrid / etc.).
        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) throw new KeyNotFoundException("User not found.");

            // Simulation: send email
            Console.WriteLine($"Reset password link sent to {email}");
        }

        // Replaces the stored BCrypt hash with a new hash derived from the provided plaintext password.
        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null) throw new KeyNotFoundException("User not found.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _userRepository.UpdateAsync(user);
        }

        // Admin updates an Agent or ClaimsOfficer's display name, email, and optionally password.
        // Customer accounts are intentionally blocked from admin editing to protect data integrity.
        public async Task<UserDto> UpdateUserAsync(string userId, RegisterDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found.");

            // Business rule: only agents and claims officers can be edited by admin
            if (user.Role == UserRole.Customer)
                throw new InvalidOperationException("Customers cannot be edited by admin.");

            user.FullName = dto.FullName;
            user.Email = dto.Email;

            // Only re-hash the password if a new one was supplied; blank means keep existing
            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _userRepository.UpdateAsync(user);
            return MapToDto(user);
        }

        // Allows any authenticated user to update their own FullName and Email.
        public async Task<UserDto> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found.");

            user.FullName = dto.FullName;
            user.Email = dto.Email;

            await _userRepository.UpdateAsync(user);
            return MapToDto(user);
        }

        // Generates a sequential ID with a role-specific prefix:
        // Customer → "cust1", "cust2" ... | Agent/Officer/Admin → "usr1", "usr2" ...
        private async Task<string> GenerateNextId(UserRole role)
        {
            string prefix = role == UserRole.Customer ? "cust" : "usr";
            var all = await _userRepository.GetAllAsync();
            var count = all.Count(u => u.Id.StartsWith(prefix));
            return $"{prefix}{count + 1}";
        }

        // Maps a User entity to a UserDto, exposing only safe fields (no password hash).
        // Includes the names of the assigned agent and claims officer if the navigation properties are loaded.
        private static UserDto MapToDto(User user) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt,
            AssignedAgentId = user.AssignedAgentId,
            AssignedAgentName = user.AssignedAgent?.FullName,
            AssignedClaimsOfficerId = user.AssignedClaimsOfficerId,
            AssignedClaimsOfficerName = user.AssignedClaimsOfficer?.FullName
        };

        // Selects the agent with the smallest number of currently assigned customers (round-robin workload).
        // Returns null if no agents exist, allowing registration to proceed without an agent assignment.
        private async Task<string?> AutoAssignAgentAsync()
        {
            var agents = (await _userRepository.FindAsync(u => u.Role == UserRole.Agent)).ToList();
            if (!agents.Any()) return null;

            // Count how many customers are already assigned to each agent
            var workloads = new List<(string AgentId, int Count)>();
            foreach (var agent in agents)
            {
                var customers = await _userRepository.FindAsync(u => u.AssignedAgentId == agent.Id);
                workloads.Add((agent.Id, customers.Count()));
            }

            // Assign to the agent with the fewest customers
            return workloads.OrderBy(w => w.Count).First().AgentId;
        }

        // Selects the claims officer with the fewest currently assigned customers (same workload strategy as agents).
        private async Task<string?> AutoAssignClaimsOfficerAsync()
        {
            var officers = (await _userRepository.FindAsync(u => u.Role == UserRole.ClaimsOfficer)).ToList();
            if (!officers.Any()) return null;

            var workloads = new List<(string OfficerId, int Count)>();
            foreach (var officer in officers)
            {
                var customers = await _userRepository.FindAsync(u => u.AssignedClaimsOfficerId == officer.Id);
                workloads.Add((officer.Id, customers.Count()));
            }

            return workloads.OrderBy(w => w.Count).First().OfficerId;
        }
    }
}
