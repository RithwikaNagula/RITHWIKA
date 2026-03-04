// Provides core functionality and structures for the application.
using Domain.Enums;

namespace Domain.Entities
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Auto-Assignment for Customers
        public string? AssignedAgentId { get; set; }
        public User? AssignedAgent { get; set; }

        public string? AssignedClaimsOfficerId { get; set; }
        public User? AssignedClaimsOfficer { get; set; }

        // Navigation properties
        public ICollection<Policy> CustomerPolicies { get; set; } = new List<Policy>();
        public ICollection<Policy> AgentPolicies { get; set; } = new List<Policy>();
        public ICollection<Policy> CreatedPolicies { get; set; } = new List<Policy>();
        public ICollection<Claim> ReviewedClaims { get; set; } = new List<Claim>();
        public ICollection<ClaimHistoryLog> ChangedClaimLogs { get; set; } = new List<ClaimHistoryLog>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        // For Agent/Officer to see their assigned customers
        public ICollection<User> AssignedCustomers { get; set; } = new List<User>();
        public ICollection<User> AssignedOfficerCustomers { get; set; } = new List<User>();
    }
}
