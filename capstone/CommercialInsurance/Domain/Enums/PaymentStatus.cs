// Indicates whether a payment is Pending, Completed, or Failed.
namespace Domain.Enums
{
    public enum PaymentStatus
    {
        Pending,    // Payment scheduled but not yet processed
        Completed,  // Payment successfully processed and recorded
        Failed      // Payment attempt was unsuccessful
    }
}
