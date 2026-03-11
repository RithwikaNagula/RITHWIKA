// Tracks policy progression: QuoteGenerated → QuoteAccepted → PendingReview → Approved → Active → Expired.
namespace Domain.Enums
{
    public enum PolicyStatus
    {
        QuoteGenerated,  // Premium quote has been calculated but not yet accepted by the customer
        QuoteAccepted,   // Customer accepted the quote; ready for policy creation
        PendingReview,   // Policy submitted and awaiting agent review
        PendingPayment,  // Policy approved; waiting for the first premium payment
        Approved,        // Agent has approved the policy application
        Active,          // Policy is in force; coverage period has begun
        Expired,         // Policy end date has passed without renewal
        Cancelled,       // Policy was cancelled before or during the coverage period
        Rejected         // Agent rejected the policy application with a reason
    }
}
