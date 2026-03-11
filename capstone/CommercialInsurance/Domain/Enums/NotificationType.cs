// Categorises notifications by event type: PolicyApproved, ClaimSubmitted, PaymentDue, etc.
namespace Domain.Enums
{
    public enum NotificationType
    {
        Policy,   // Notifications related to policy creation, approval, or rejection
        Claim,    // Notifications about claim status changes
        Payment,  // Payment confirmations and upcoming due date reminders
        System,   // Administrative or system-wide announcements
        Renewal   // Alerts about policy renewal eligibility or auto-renewal events
    }
}
