// Represents a system user (Customer, Agent, ClaimsOfficer, or Admin) with BCrypt-hashed password and role-based access.
using Domain.Enums;

namespace Domain.Entities
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        // Unique email used as the login credential
        public string Email { get; set; } = string.Empty;
        // BCrypt-hashed password; never stored or transmitted in plain text
        public string PasswordHash { get; set; } = string.Empty;
        // System role controlling access: Admin, Customer, Agent, or ClaimsOfficer
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Auto-Assignment for Customers
        // The agent auto-assigned to this customer via round-robin during registration
        public string? AssignedAgentId { get; set; }
        public User? AssignedAgent { get; set; }

        // The claims officer auto-assigned to this customer via round-robin during registration
        public string? AssignedClaimsOfficerId { get; set; }
        public User? AssignedClaimsOfficer { get; set; }

        // Navigation: policies where this user is the owning customer
        public ICollection<Policy> CustomerPolicies { get; set; } = new List<Policy>();
        // Navigation: policies where this user is the assigned agent
        public ICollection<Policy> AgentPolicies { get; set; } = new List<Policy>();
        // Navigation: policies created by this user (overlaps with CustomerPolicies in most cases)
        public ICollection<Policy> CreatedPolicies { get; set; } = new List<Policy>();
        // Navigation: claims reviewed by this user (ClaimsOfficer role)
        public ICollection<Claim> ReviewedClaims { get; set; } = new List<Claim>();
        public ICollection<ClaimHistoryLog> ChangedClaimLogs { get; set; } = new List<ClaimHistoryLog>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        // Inverse navigation: customers assigned to this agent
        public ICollection<User> AssignedCustomers { get; set; } = new List<User>();
        // Inverse navigation: customers assigned to this claims officer
        public ICollection<User> AssignedOfficerCustomers { get; set; } = new List<User>();
    }
}
