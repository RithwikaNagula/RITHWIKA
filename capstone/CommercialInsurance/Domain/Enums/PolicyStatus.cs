// Provides core functionality and structures for the application.
namespace Domain.Enums
{
    public enum PolicyStatus
    {
        QuoteGenerated,
        QuoteAccepted,
        PendingReview,
        PendingPayment,
        Approved,
        Active,
        Expired,
        Cancelled,
        Rejected
    }
}
