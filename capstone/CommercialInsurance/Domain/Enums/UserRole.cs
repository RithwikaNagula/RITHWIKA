// Defines the four user roles that control access across the system: Admin, Agent, ClaimsOfficer, Customer.
namespace Domain.Enums
{
    public enum UserRole
    {
        Admin,          // Full system access: manages agents, officers, insurance types, and analytics
        Customer,       // Purchases policies, submits claims, and makes premium payments
        Agent,          // Reviews and approves/rejects policy applications; earns commission
        ClaimsOfficer   // Reviews and settles insurance claims filed by customers
    }
}
