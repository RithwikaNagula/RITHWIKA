// Provides core functionality and structures for the application.
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

        public AuthService(
            IUserRepository userRepository,
            JwtTokenGenerator jwtTokenGenerator,
            INotificationService notificationService,
            IPolicyRepository policyRepository,
            IClaimRepository claimRepository)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _notificationService = notificationService;
            _policyRepository = policyRepository;
            _claimRepository = claimRepository;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            var token = _jwtTokenGenerator.GenerateToken(user);

            return new LoginResponseDto
            {
                Token = token,
                User = MapToDto(user)
            };
        }

        public async Task<UserDto> RegisterAsync(RegisterDto dto)
        {
            var existing = await _userRepository.GetByEmailAsync(dto.Email);
            if (existing != null)
                throw new InvalidOperationException("Email is already registered.");

            var id = await GenerateNextId(UserRole.Customer);
            var user = new User
            {
                Id = id,
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow,
                AssignedAgentId = await AutoAssignAgentAsync(),
                AssignedClaimsOfficerId = await AutoAssignClaimsOfficerAsync()
            };

            await _userRepository.AddAsync(user);

            if (!string.IsNullOrEmpty(user.AssignedAgentId))
            {
                await _notificationService.CreateNotificationAsync(
                    user.AssignedAgentId,
                    "New Customer Assigned",
                    $"A new customer {user.FullName} has been assigned to you.",
                    NotificationType.System.ToString()
                );
            }

            if (!string.IsNullOrEmpty(user.AssignedClaimsOfficerId))
            {
                await _notificationService.CreateNotificationAsync(
                    user.AssignedClaimsOfficerId,
                    "New Customer Assigned",
                    $"A new customer {user.FullName} has been assigned to you.",
                    NotificationType.System.ToString()
                );
            }

            return MapToDto(user);
        }

        public async Task<UserDto> CreateUserWithRoleAsync(RegisterDto dto, string role)
        {
            var existing = await _userRepository.GetByEmailAsync(dto.Email);
            if (existing != null)
                throw new InvalidOperationException("Email is already registered.");

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

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role)
        {
            if (!Enum.TryParse<UserRole>(role, true, out var userRole))
                throw new ArgumentException($"Invalid role: {role}");

            var users = await _userRepository.FindAsync(u => u.Role == userRole);
            return users.Select(MapToDto);
        }

        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null ? MapToDto(user) : null;
        }

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

                // Reassign customers
                var assignedCustomers = await _userRepository.FindAsync(u => u.AssignedAgentId == userId);
                foreach (var customer in assignedCustomers)
                {
                    var tracked = await _userRepository.GetByIdAsync(customer.Id);
                    if (tracked != null) { tracked.AssignedAgentId = newAgentId; await _userRepository.UpdateAsync(tracked); }
                }

                // Reassign policies
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
                    var workloads = new List<(string OfficerId, int Count)>();
                    foreach (var officer in otherOfficers)
                    {
                        var count = (await _claimRepository.FindAsync(c => c.ClaimsOfficerId == officer.Id)).Count();
                        workloads.Add((officer.Id, count));
                    }
                    newOfficerId = workloads.OrderBy(w => w.Count).First().OfficerId;
                }

                // Reassign customers
                var assignedCustomers = await _userRepository.FindAsync(u => u.AssignedClaimsOfficerId == userId);
                foreach (var customer in assignedCustomers)
                {
                    var tracked = await _userRepository.GetByIdAsync(customer.Id);
                    if (tracked != null) { tracked.AssignedClaimsOfficerId = newOfficerId; await _userRepository.UpdateAsync(tracked); }
                }

                // Reassign claims
                var officerClaims = await _claimRepository.FindAsync(c => c.ClaimsOfficerId == userId);
                foreach (var claim in officerClaims)
                {
                    var trackedClaim = await _claimRepository.GetByIdAsync(claim.Id);
                    if (trackedClaim != null) { trackedClaim.ClaimsOfficerId = newOfficerId; await _claimRepository.UpdateAsync(trackedClaim); }
                }
            }

            await _userRepository.DeleteAsync(user);
            return true;

        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) throw new KeyNotFoundException("User not found.");

            // Simulation: send email
            Console.WriteLine($"Reset password link sent to {email}");
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null) throw new KeyNotFoundException("User not found.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _userRepository.UpdateAsync(user);
        }

        public async Task<UserDto> UpdateUserAsync(string userId, RegisterDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found.");

            // Logic: Edit Agent and Claims Officer but NOT Customer (requested by user)
            if (user.Role == UserRole.Customer)
                throw new InvalidOperationException("Customers cannot be edited by admin.");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _userRepository.UpdateAsync(user);
            return MapToDto(user);
        }

        private async Task<string> GenerateNextId(UserRole role)
        {
            string prefix = role == UserRole.Customer ? "cust" : "usr";
            var all = await _userRepository.GetAllAsync();
            var count = all.Count(u => u.Id.StartsWith(prefix));
            return $"{prefix}{count + 1}";
        }

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

        private async Task<string?> AutoAssignAgentAsync()
        {
            var agents = (await _userRepository.FindAsync(u => u.Role == UserRole.Agent)).ToList();
            if (!agents.Any()) return null;

            // Simple workload: count of currently assigned customers
            var workloads = new List<(string AgentId, int Count)>();
            foreach (var agent in agents)
            {
                var customers = await _userRepository.FindAsync(u => u.AssignedAgentId == agent.Id);
                workloads.Add((agent.Id, customers.Count()));
            }

            return workloads.OrderBy(w => w.Count).First().AgentId;
        }

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
