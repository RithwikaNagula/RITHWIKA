// Tracks whether an agent's commission for a policy is Pending or Paid.
namespace Domain.Enums
{
    public enum CommissionStatus
    {
        Pending,    // Commission calculated but not yet released
        Earned,     // Policy has been activated and commission is confirmed
        Paid,       // Commission amount has been disbursed to the agent
        Cancelled   // Commission voided due to policy cancellation or rejection
    }
}
